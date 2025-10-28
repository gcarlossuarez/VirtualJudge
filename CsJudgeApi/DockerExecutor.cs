using System.Diagnostics;
using CsJudgeApi.Models;

namespace CsJudgeApi;

public static class DockerExecutor
{
    public static (bool flowControl, IResult value) Execute(string language, int timeLimit, string validatorDir, string problemsRoot, int problemId, Question question, string volumeInSpec, string volumeOutSpec, out string stdout, out string stderr, string srcDir)
    {
        Process proc;
        Console.WriteLine("Iniciando ejecución con Docker...");
        var psi = new ProcessStartInfo
        {
            FileName = "docker",
            RedirectStandardOutput = true,
            RedirectStandardError = true,
            UseShellExecute = false
        };
        psi.ArgumentList.Add("run");
        psi.ArgumentList.Add("--rm");                // 🔹 El contenedor se borra al terminar → no deja basura
        psi.ArgumentList.Add("--network=none");      // 🔹 Sin acceso a internet
        psi.ArgumentList.Add("--cpus=1");            // 🔹 Limita a 1 CPU
        psi.ArgumentList.Add("--memory=1536m");      // 🔹 Limita a 1.5 GB de RAM
                                                    //psi.ArgumentList.Add("--memory=3072m");      // 🔹 Limita a 3 GB de RAM
        psi.ArgumentList.Add("--pids-limit=256");    // 🔹 Evita fork bombs (máx. 256 procesos dentro)


        // IN (readonly)
        psi.ArgumentList.Add("-v");
        psi.ArgumentList.Add(volumeInSpec);            // 🔹 Monta srcDir como /home/sandbox/in (readonly, por `:ro`)

        // OUT (Lectura/escritura)
        psi.ArgumentList.Add("-v");
        psi.ArgumentList.Add(volumeOutSpec);

        // Se obtiene la ruta completa del programa validador, para la pregunta, si existe
        string fullPathValidator = question.FullPathValidatorSourceCode;
        if (!string.IsNullOrEmpty(fullPathValidator) &&
        Environment.GetEnvironmentVariable("PROBLEMS_PATH") != null)
        {
            fullPathValidator = Path.Combine(problemsRoot, problemId.ToString(), $"Validator{problemId}");
            fullPathValidator = Path.Combine(fullPathValidator, "Validator.cs"); // por ahora, solo cs
        }

        // Si esta definido el validador
        if (!string.IsNullOrEmpty(fullPathValidator))
        {
            // Copia el validador en el directorio auxiliar
            string fullFilePathValidatorTmp = Path.Combine(validatorDir, new FileInfo(fullPathValidator).Name);
            File.Copy(fullPathValidator, fullFilePathValidatorTmp);

            var chmod = new ProcessStartInfo
            {
                FileName = "chmod",
                ArgumentList = { "a+r", fullFilePathValidatorTmp },
                RedirectStandardOutput = true,
                RedirectStandardError = true,
                UseShellExecute = false
            };

            using (var procChmodValidatorTmpFile = Process.Start(chmod))
            {
                if (procChmodValidatorTmpFile != null)
                {
                    procChmodValidatorTmpFile.WaitForExit();
                    Console.WriteLine($"Se ajustaron permisos de lectura a: {fullFilePathValidatorTmp}");
                }
                else
                {
                    proc = new Process();
                    stdout = "";
                    stderr = "";
                    return (flowControl: false, value: Results.BadRequest($"No se pudo iniciar el proceso chmod para: {fullFilePathValidatorTmp}"));
                }
            }

            // Montar el archivo de validador desde el host al contenedor
            psi.ArgumentList.Add("-v");
            psi.ArgumentList.Add($"{Path.GetDirectoryName(fullFilePathValidatorTmp)}:/home/sandbox/validator");
        }

        // Extras adicionados, para mayor seguridad
        psi.ArgumentList.Add("--read-only"); // hace que todo el FS dentro del contenedor sea solo lectura (excepto volúmenes montados).


        // Directorio temporal para escritura
        psi.ArgumentList.Add("--tmpfs");
        psi.ArgumentList.Add("/home/sandbox/tmp:exec");

        // Directorio temporal para no tener que setear "DOTNET_SKIP_FIRST_TIME_EXPERIENCE"
        psi.ArgumentList.Add("--tmpfs");
        psi.ArgumentList.Add("/home/sandbox/.dotnet:exec");

        // Si necesitan escribir en /tmp, lo hace en RAM y desaparece al terminar. 
        psi.ArgumentList.Add("--tmpfs");
        psi.ArgumentList.Add("/tmp");

        // Elimina todas las “capacidades” de Linux (ej: no puede manipular red, montar FS, etc.).
        psi.ArgumentList.Add("--cap-drop=ALL");

        // Asegura que no puedan elevar permisos ni con exploits.
        psi.ArgumentList.Add("--security-opt");
        psi.ArgumentList.Add("no-new-privileges");

        psi.ArgumentList.Add("cs-single-runner:1");  // 🔹 Imagen que compila/ejecuta el C#
        psi.ArgumentList.Add("/home/sandbox/in");    // 🔹 Directorio dentro del contenedor
        psi.ArgumentList.Add(timeLimit.ToString());  // 🔹 Pasa el límite de tiempo como parámetro


        #region Lenguaje
        // Se pasa el lenguaje, como argumento
        var languageMap = new Dictionary<string, string>
        {
            ["csharp"] = "dotnet",
            ["cpp"] = "g++"
            // después agregás python, java, etc.
        };

        if (!languageMap.TryGetValue(language, out var langBin))
        {
            langBin = "dotnet"; // default
        }

        psi.ArgumentList.Add(langBin);
        #endregion


        #region Validador
        // Si esta definido el validador, se lo pasas como argumento
        if (!string.IsNullOrEmpty(fullPathValidator))
        {
            // argumento al script
            psi.ArgumentList.Add("/home/sandbox/validator/Validator.cs");
        }

        // Depurar las variables de entorno que toma docker
        //Console.WriteLine("docker " + string.Join(" ", psi.ArgumentList));
        //foreach (var env in psi.Environment)
        //{
        //    Console.WriteLine($"ENV: {env.Key}={env.Value}");
        //}
        #endregion


        var startedAt = DateTimeOffset.UtcNow;
        proc = Process.Start(psi)!;
        stdout = proc.StandardOutput.ReadToEnd();
        stderr = proc.StandardError.ReadToEnd();
        proc.WaitForExit(120_000);
        return (flowControl: true, value: Results.Ok());
    }
}