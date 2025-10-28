using System.Diagnostics;
using CsJudgeApi.Models;

namespace CsJudgeApi;

public static class NsjailExecutor
{
    public static (bool flowControl, IResult value) Execute(string language, int timeLimit, string validatorDir, string problemsRoot, int problemId, Question question, string volumeInSpec, string volumeOutSpec, out string stdout, out string stderr, string srcDir)
    {
        Process proc;
        stdout = "";
        stderr = "";

        //throw new NotImplementedException();
        Console.WriteLine("Iniciando ejecución con nsjail...");

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
        }

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
        
        // Si el lenguaje es dotnet, crear estructura básica del proyecto
        if (langBin == "dotnet")
        {
            CreateDotNetProjectTemplate();
        }
        #endregion

        string parentDir = Directory.GetParent(srcDir)?.FullName ?? "";

        // Obtener el nombre del directorio (última parte de la ruta)
        string dirName = Path.GetFileName(srcDir);
        
        // Crear el directorio destino
        string destDir = Path.Combine("/tmp/temp-workdir", dirName);
        
        // Crear el directorio destino si no existe
        Directory.CreateDirectory(destDir);
        
        // Copiar todo el contenido de srcDir al directorio destino
        CopyDirectory(srcDir, destDir, true);
        
        string arguments = $"bash /home/vboxuser/VirtualJudge/nsjail-runner/run_single.sh /tmp/temp-workdir /tmp/temp-workdir/{dirName} {timeLimit} {langBin} {fullPathValidator.Trim()}";

        Console.WriteLine($"Comando nsjail: sudo {arguments}");
        var process = new Process
        {
            StartInfo = new ProcessStartInfo
            {
                FileName = "sudo",
                Arguments = arguments,
                UseShellExecute = false,  // No interactivo, no pide pass
                RedirectStandardOutput = true,
                RedirectStandardError = true,
                CreateNoWindow = true
            }
        };

        var startedAt = DateTimeOffset.UtcNow;
        process.Start();
        stdout = process.StandardOutput.ReadToEnd();  // Captura RUN/SUMMARY
        stderr = process.StandardError.ReadToEnd();   // Captura DETAILS/errores
        process.WaitForExit(60_000); // Espera a que termine o se produzca TimeOut
        
        return (flowControl: true, value: Results.Ok());
    }
    
    /// <summary>
    /// Crea la estructura básica de un proyecto .NET en /tmp/temp-workdir/template/App
    /// Con control de concurrencia para evitar conflictos entre procesos
    /// </summary>
    private static void CreateDotNetProjectTemplate()
    {
        string projectDir = "/tmp/temp-workdir/template/App";
        string csprojPath = Path.Combine(projectDir, "Solution.csproj");
        string lockFilePath = Path.Combine("/tmp/temp-workdir", ".dotnet-template.lock");
        
        // Verificar si el archivo ya existe
        if (File.Exists(csprojPath))
        {
            Console.WriteLine($"ℹ️  Proyecto .NET ya existe en: {projectDir}");
            return;
        }
        
        FileStream? lockFile = null;
        try
        {
            // Crear directorio para el lock file si no existe
            Directory.CreateDirectory(Path.GetDirectoryName(lockFilePath)!);
            
            // Intentar obtener el lock de manera no bloqueante
            lockFile = new FileStream(lockFilePath, FileMode.Create, FileAccess.Write, FileShare.None);
            
            // Si llegamos aquí, obtuvimos el lock exitosamente
            Console.WriteLine($"🔒 Lock obtenido para crear proyecto .NET");
            
            // Verificar nuevamente si el archivo existe (double-check locking pattern)
            if (File.Exists(csprojPath))
            {
                Console.WriteLine($"ℹ️  Proyecto .NET ya fue creado por otro proceso: {projectDir}");
                return;
            }
            
            // Crear el directorio del proyecto
            Directory.CreateDirectory(projectDir);
            
            // Contenido del archivo Solution.csproj
            string csprojContent = @"<Project Sdk=""Microsoft.NET.Sdk"">

  <PropertyGroup>
    <OutputType>Exe</OutputType>
    <TargetFramework>net8.0</TargetFramework>
  </PropertyGroup>

</Project>";
            
            // Crear el archivo Solution.csproj
            File.WriteAllText(csprojPath, csprojContent);
            
            Console.WriteLine($"✅ Proyecto .NET creado exitosamente en: {projectDir}");
            Console.WriteLine($"   - Solution.csproj creado");
        }
        catch (IOException ex) when (ex.Message.Contains("being used by another process") || 
                                   ex.Message.Contains("already exists"))
        {
            // No se pudo obtener el lock - otro proceso ya lo tiene
            Console.WriteLine($"⏭️  Otro proceso está creando el proyecto .NET, saltando...");
            return;
        }
        catch (Exception ex)
        {
            Console.WriteLine($"❌ Error creando proyecto .NET: {ex.Message}");
        }
        finally
        {
            // Liberar el lock y limpiar el archivo de lock
            try
            {
                lockFile?.Close();
                lockFile?.Dispose();
                
                if (File.Exists(lockFilePath))
                {
                    File.Delete(lockFilePath);
                    Console.WriteLine($"🔓 Lock liberado");
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"⚠️  Error liberando lock: {ex.Message}");
            }
        }
    }
    
    /// <summary>
    /// Copia un directorio y todo su contenido de forma recursiva
    /// </summary>
    /// <param name="sourceDir">Directorio fuente</param>
    /// <param name="destinationDir">Directorio destino</param>
    /// <param name="recursive">Si debe copiar subdirectorios recursivamente</param>
    private static void CopyDirectory(string sourceDir, string destinationDir, bool recursive)
    {
        // Obtener información del directorio fuente
        var dir = new DirectoryInfo(sourceDir);

        // Verificar que el directorio fuente existe
        if (!dir.Exists)
            throw new DirectoryNotFoundException($"Directorio fuente no encontrado: {dir.FullName}");

        // Crear el directorio destino si no existe
        Directory.CreateDirectory(destinationDir);

        // Copiar archivos del directorio actual
        foreach (FileInfo file in dir.GetFiles())
        {
            string targetFilePath = Path.Combine(destinationDir, file.Name);
            file.CopyTo(targetFilePath, true); // true = sobrescribir si existe
        }

        // Si es recursivo, copiar subdirectorios
        if (recursive)
        {
            foreach (DirectoryInfo subDir in dir.GetDirectories())
            {
                string newDestinationDir = Path.Combine(destinationDir, subDir.Name);
                CopyDirectory(subDir.FullName, newDestinationDir, true);
            }
        }
    }
}