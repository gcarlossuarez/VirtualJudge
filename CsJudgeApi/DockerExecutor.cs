using System.Diagnostics;
using CsJudgeApi.Models;

namespace CsJudgeApi;

public static class DockerExecutor
{
    public static (bool flowControl, IResult value) Execute(string language, int timeLimit, string validatorDir, string problemsRoot, int problemId, Question question, string volumeInSpec, string volumeOutSpec, out string stdout, out string stderr, string srcDir)
    {
        stdout = "";
        stderr = "";
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
        string? validatorDllPath = string.Empty;
        string tempValidatorDir = string.Empty;
        try
        {
            if (!string.IsNullOrEmpty(fullPathValidator))
            {
                // Si está definido el validador, compílarlo en un directorio temporal y
                // montar el DLL
                
                // Cambiar la ubicación del directorio temporal para el validador
                string baseValidatorTmp = Path.Combine(Directory.GetCurrentDirectory(), "tmp_validators");
                Directory.CreateDirectory(baseValidatorTmp); // Asegura que exista
                if (!string.IsNullOrEmpty(fullPathValidator))
                {
                    tempValidatorDir = Path.Combine(baseValidatorTmp, $"validator_{problemId}_{Guid.NewGuid()}");
                    Console.WriteLine($"[Validador] Creando directorio temporal: {tempValidatorDir}");
                    Directory.CreateDirectory(tempValidatorDir);
                    string validatorSource = Path.Combine(tempValidatorDir, "Validator.cs");
                    File.Copy(fullPathValidator, validatorSource, true);
                    Console.WriteLine($"[Validador] Copiado Validator.cs a: {validatorSource}");

                    // Si existe un .csproj en el mismo directorio que el Validator.cs original, lo copiamos manteniendo su nombre
                    string? originalCsproj = Directory.GetFiles(Path.GetDirectoryName(fullPathValidator) ?? "", "*.csproj").FirstOrDefault();
                    string csprojPath;
                    if (!string.IsNullOrEmpty(originalCsproj))
                    {
                        csprojPath = Path.Combine(tempValidatorDir, Path.GetFileName(originalCsproj));
                        File.Copy(originalCsproj, csprojPath, true);
                        Console.WriteLine($"[Validador] Copiado csproj original a: {csprojPath}");
                    }
                    else
                    {
                        csprojPath = Path.Combine(tempValidatorDir, "ValidatorApp.csproj");
                        Console.WriteLine($"[Validador] Generando csproj nuevo en: {csprojPath}");
                        // Si no existe, generamos uno nuevo como antes
                        File.WriteAllText(csprojPath, @"
<Project Sdk=""Microsoft.NET.Sdk"">
    <PropertyGroup>
        <OutputType>Exe</OutputType>
        <TargetFramework>net8.0</TargetFramework>
        <ImplicitUsings>enable</ImplicitUsings>
        <Nullable>enable</Nullable>
    </PropertyGroup>
    <ItemGroup>
        <PackageReference Include=""Microsoft.CodeAnalysis.CSharp"" Version=""4.8.0"">
            <ExcludeAssets>analyzers</ExcludeAssets>
        </PackageReference>
    </ItemGroup>
</Project>");
                    }

                    // Restore y build
                    Console.WriteLine($"[Validador] Ejecutando dotnet restore en: {csprojPath}");
                    var restoreProc = Process.Start(new ProcessStartInfo {
                            FileName = "dotnet",
                            Arguments = $"restore \"{csprojPath}\"",
                            WorkingDirectory = tempValidatorDir,
                            RedirectStandardOutput = true,
                            RedirectStandardError = true,
                            UseShellExecute = false
                    });
                    if(restoreProc != null)
                    {
                        restoreProc.WaitForExit();
                        Console.WriteLine($"[Validador] dotnet restore finalizado. Ejecutando dotnet build en: {csprojPath}");
                        var buildProc = Process.Start(new ProcessStartInfo {
                                FileName = "dotnet",
                                Arguments = $"build \"{csprojPath}\" -c Release --no-restore",
                                WorkingDirectory = tempValidatorDir,
                                RedirectStandardOutput = true,
                                RedirectStandardError = true,
                                UseShellExecute = false
                        });
                        var buildLogPath = Path.Combine(tempValidatorDir, "build.log");
                        if(buildProc != null)
                        {
                            using (var buildLog = new StreamWriter(buildLogPath, false))
                            {
                                buildLog.WriteLine(buildProc.StandardOutput.ReadToEnd());
                                buildLog.WriteLine(buildProc.StandardError.ReadToEnd());
                            }
                        }
                        // Buscar el nombre del proyecto en el .csproj
                        string projectName = "ValidatorApp";
                        try
                        {
                            var csprojContent = File.ReadAllText(csprojPath);
                            var match = System.Text.RegularExpressions.Regex.Match(csprojContent, @"<AssemblyName>(.*?)</AssemblyName>");
                            if (match.Success)
                            {
                                projectName = match.Groups[1].Value.Trim();
                            }
                            else
                            {
                                // Si no hay AssemblyName, usar el nombre del archivo .csproj
                                projectName = Path.GetFileNameWithoutExtension(csprojPath);
                            }
                        }
                        catch { /* Si falla, usar el default */ }
                        string expectedDll = Path.Combine(tempValidatorDir, "bin", "Release", "net8.0", projectName + ".dll");
                        if (File.Exists(expectedDll))
                        {
                            validatorDllPath = expectedDll;
                        }
                        else
                        {
                            // Fallback: buscar cualquier DLL
                            validatorDllPath = Directory.GetFiles(Path.Combine(tempValidatorDir, "bin", "Release", "net8.0"), "*.dll").FirstOrDefault();
                        }
                        if (validatorDllPath == null)
                        {
                            string buildLogContent = File.Exists(buildLogPath) ? File.ReadAllText(buildLogPath) : "(No se encontró build.log)";
                            Console.WriteLine($"[Validador] ERROR: No se pudo compilar el validador en {tempValidatorDir}\nBuild log:\n{buildLogContent}");
                            proc = new Process();
                            stdout = "";
                            stderr = buildLogContent;
                            return (flowControl: false, value: Results.BadRequest($"No se pudo compilar el validador en {tempValidatorDir}\nBuild log:\n{buildLogContent}"));
                        }
                        Console.WriteLine($"[Validador] Compilación exitosa. DLL generado en: {validatorDllPath}");
                        // Monta el directorio temporal con el DLL en el contenedor
                        psi.ArgumentList.Add("-v");
                        psi.ArgumentList.Add($"{tempValidatorDir}:/home/sandbox/validator:ro");
                    
                    }
                }
            }

            // 🔹 Montar la plantilla base del contenedor (solo lectura)
            //    Esto evita el Permission denied al copiar desde /home/sandbox/template/App
            // 📝 El template, ya se construye en la imagen Docker. Si no se comenta esa
            // instrucción, se monta un directorio que oculta al directorio de la imagen Docker
            // y, encima, se monta con privilegios de root; lo cual, impíde que el usuario 
            // "sandbox" pueda trabajr libremente, en él.
            //psi.ArgumentList.Add("-v");
            //psi.ArgumentList.Add("/home/virtualbox/VirtualJudge/template:/home/sandbox/template:ro");

            // Extras adicionados, para mayor seguridad
            psi.ArgumentList.Add("--read-only"); // hace que todo el FS dentro del contenedor sea solo lectura (excepto volúmenes montados).


            // Directorio temporal para escritura
            psi.ArgumentList.Add("--tmpfs");
            //psi.ArgumentList.Add("/home/sandbox/tmp:exec");
            psi.ArgumentList.Add("/home/sandbox/tmp:exec,mode=777,size=128M");


            int uidDocker = 1655;
            // Directorio temporal para no tener que setear "DOTNET_SKIP_FIRST_TIME_EXPERIENCE"
            //psi.ArgumentList.Add("/home/sandbox/.dotnet:exec");
            psi.ArgumentList.Add("--tmpfs");
            psi.ArgumentList.Add($"/home/sandbox/.dotnet:exec,mode=777,uid={uidDocker},gid={uidDocker},size=64M");

            // Directorio temporal para las plantillas de dotnet new
            psi.ArgumentList.Add("--tmpfs");
            psi.ArgumentList.Add($"/home/sandbox/.templateengine:exec,mode=777,uid={uidDocker},gid={uidDocker},size=64M");


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
            // Si esta definido el validador, se lo pasa como último argumento
            bool hasValidator = !string.IsNullOrEmpty(fullPathValidator) && !string.IsNullOrEmpty(validatorDllPath);

            // El validador debe ir como último argumento
            if (hasValidator)
            {
                // Pasa la ruta completa y real dentro del contenedor
                psi.ArgumentList.Add($"/home/sandbox/validator/bin/Release/net8.0/{Path.GetFileName(validatorDllPath)}");
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
            proc.WaitForExit(60_000); // Espera a que termine o se produzca TimeOut
            return (flowControl: true, value: Results.Ok());
        }
        finally
        {
            // Eliminar el directorio temporal del validador si fue creado
            if (!string.IsNullOrEmpty(tempValidatorDir) && Directory.Exists(tempValidatorDir))
            {
                try { Directory.Delete(tempValidatorDir, true); } catch { /* Ignorar errores de borrado */ }
            }
        }
    }

    // Si por alguna razón se sale del flujo principal, retornar error genérico
    public static (bool flowControl, IResult value) ErrorResult(out string stdout, out string stderr)
    {
        stdout = "";
        stderr = "Error inesperado en DockerExecutor";
        return (false, Results.BadRequest("Error inesperado en DockerExecutor"));
    }
}