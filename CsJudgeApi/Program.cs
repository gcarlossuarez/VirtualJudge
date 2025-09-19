using System.IO.Compression;
using System.Text;
using System.Text.Json;
using System.Text.RegularExpressions;
using System.Diagnostics;
using Microsoft.EntityFrameworkCore;
using CsJudgeApi.Data;
using CsJudgeApi.Models;
using Xceed.Words.NET; // ⚡ DocX. Para instalar, hacer => dotnet add package DocX --version 1.0.0

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowAll", policy =>
    {
        policy.AllowAnyOrigin()
              .AllowAnyMethod()
              .AllowAnyHeader();
    });
});


// EF Core con SQLite
builder.Services.AddDbContext<AppDbContext>(options =>
    options.UseSqlite($"Data Source={PathDirectories.DB_PATH}"));

builder.WebHost.ConfigureKestrel(o =>
{
    o.Limits.MaxRequestBodySize = 20 * 1024 * 1024; // 20MB
});


builder.WebHost.UseUrls("http://0.0.0.0:5000");


var app = builder.Build();
app.UseCors("AllowAll");

// Habilitar archivos estáticos (sirve index.html desde wwwroot/)
app.UseDefaultFiles();
app.UseStaticFiles();


Contest? currentContest = null;
// Aseguramos DB creada
using (var scope = app.Services.CreateScope())
{
    var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();
    db.Database.EnsureCreated();

    // ✅ Validar que exista contest activo
    currentContest = db.Contests
                       .OrderByDescending(c => c.Date).ThenByDescending(c => c.ContestId)
                       .FirstOrDefault();
    Console.WriteLine("ContestId=" + currentContest?.ContestId + " Date=" + currentContest?.Date);
}


// NOTA. / Flujo resumido (con DbContext)
// 1. Al iniciar la app, registra AppDbContext en el contenedor DI.
// 2. En cada request, ASP.NET Core crea una instancia nueva de AppDbContext (scoped).
// 3. Esa instancia vive solo durante el request y se libera al terminar.
// No se puede usar db fuera de un endpoint → no existe como variable global, solo se obtiene por parámetro.

// === ENDPOINT SUBMIT (registra envío y compara con expected) ===
app.MapPost("/submit", async (AppDbContext db, HttpContext ctx) =>
{
    var form = await ctx.Request.ReadFormAsync();
    long.TryParse(form["studentId"], out long studentId);
    var problemId = form["problemId"].ToString();
    var source = form["sourceCode"].ToString();
    var language = form["language"].ToString();
    var expected = form["expected"].ToString();
    var actual = form["actual"].ToString();

    var submission = new Submission
    {
        StudentId = studentId,
        ProblemId = problemId,
        SourceCode = source,
        OutputExpected = expected,
        OutputActual = actual,
        IsCorrect = Normalize(expected) == Normalize(actual)
    };

    db.Submissions.Add(submission);
    await db.SaveChangesAsync();

    return Results.Json(submission);

    static string Normalize(string s) => s.Replace("\r\n", "\n").Trim();
});

// === ENDPOINT SALUD ===
app.MapGet("/", () => Results.Json(new { ok = true, service = "CsJudgeApi", version = "1.0" }));

app.MapGet("/healthz", () =>
{
    try
    {
        var psi = new ProcessStartInfo
        {
            FileName = "docker",
            ArgumentList = { "--version" },
            RedirectStandardOutput = true,
            RedirectStandardError = true,
            UseShellExecute = false
        };
        using var p = Process.Start(psi)!;
        var outp = p.StandardOutput.ReadToEnd();
        var err = p.StandardError.ReadToEnd();
        p.WaitForExit(5000);
        return Results.Json(new { ok = p.ExitCode == 0, docker = outp.Trim(), err = err.Trim() });
    }
    catch (Exception ex)
    {
        return Results.Json(new { ok = false, error = ex.Message }, statusCode: 500);
    }
});

static string GetClientIp(HttpRequest req)
{
    // 1. Si viene de ngrok o proxy, usar el header X-Forwarded-For
    if (req.Headers.TryGetValue("X-Forwarded-For", out var forwardedFor))
    {
        return forwardedFor.FirstOrDefault() ?? "::1";
    }

    // 2. Fallback: IP directa de la conexión
    return req.HttpContext.Connection.RemoteIpAddress?.ToString() ?? "::1";
}


// === ENDPOINT COMPILAR Y EJECUTAR ===
app.MapPost("/compile-run", async (HttpRequest req, AppDbContext db) =>
{
    if (!req.HasFormContentType)
        return Results.BadRequest("Envía un formulario multipart con el archivo 'code' (.cs o .zip).");

    // ✅ Obtener la IP del cliente
    var ip = GetClientIp(req);

    Console.WriteLine($"Nueva petición desde {ip}");

    var form = await req.ReadFormAsync();
    var codeFile = form.Files["code"];
    if (codeFile is null || codeFile.Length == 0)
        return Results.BadRequest("Falta el archivo 'code'.");

    var inputFile = form.Files["input"]; // opcional
    var stdinText = form["stdin"].ToString(); // opcional
    var keep = form["keep"].ToString().Equals("true", StringComparison.OrdinalIgnoreCase);
    var timeLimitStr = form["timeLimit"].ToString();
    var language = form["language"].ToString();
    int timeLimit = 6;
    if (int.TryParse(timeLimitStr, out var tl)) timeLimit = Math.Clamp(tl, 1, 30);

    var rootTmp = Path.Combine(Path.GetTempPath(), "cs_api");
    //var rootTmp = Path.Combine("/home/vboxuser/VirtualJudge/work", "cs_api");
    //var rootTmp = Path.Combine("/var/tmp", "cs_api");

    Directory.CreateDirectory(rootTmp);
    var id = Guid.NewGuid().ToString("N");
    var work = Path.Combine(rootTmp, id);
    var srcDir = work;
    var outDir = Path.Combine(work, "OUT");
    var validatorDir = Path.Combine(work, "VALIDATOR");


    // 🔹 Ajustar permisos para que cualquiera pueda leer/escribir
    // Hay un bug conocido en .NET 6/7/8 en Linux cuando se copian archivos 
    // sobre ciertos tmpfs o directorios montados en Docker con restricciones 
    // → suele resolverse con permisos explícitos (chmod 777) o copiando con
    //  FileStream en lugar de File.Copy.
    var chmodA = new ProcessStartInfo
    {
        FileName = "chmod",
        Arguments = "-R 777 " + rootTmp,
        RedirectStandardOutput = true,
        RedirectStandardError = true,
        UseShellExecute = false
    };
    using (var procA= Process.Start(chmodA))
    {
        procA.WaitForExit();
    }

    Directory.CreateDirectory(srcDir);
    Directory.CreateDirectory(outDir);
    Directory.CreateDirectory(validatorDir);
    string problemsRoot = PathDirectories.PROBLEMS_PATH;

    var chmodSrcDir = new ProcessStartInfo {
        FileName = "chmod",
        Arguments = $"-R 777 {srcDir}",
        RedirectStandardOutput = true,
        RedirectStandardError = true,
        UseShellExecute = false
    };
    using (var procSrcDir = Process.Start(chmodSrcDir)) {
        procSrcDir.WaitForExit();
    }


    int.TryParse(form["problemId"], out int problemId); // viene de la BD o del request

    long.TryParse(form["studentId"], out long studentId);

    if (currentContest is null)
        return Results.BadRequest("No hay contest activo");

    // ✅ Validar que el problema exista en ese contest
    var question = await db.Questions
        .FirstOrDefaultAsync(q => q.ContestId == currentContest.ContestId && q.QuestionId == problemId);

    if (question == null)
        return Results.BadRequest($"El problema {problemId} no pertenece al contest {currentContest.ContestId}");

    // ✅ Validar que el estudiante exista
    var student = db.Students.FirstOrDefault(s => s.StudentId == studentId);
    if (student == null)
        return Results.BadRequest($"El estudiante con numero de registro {studentId} no existe");

    Console.WriteLine("Request enviado por el estudiante " + student.Name);

    // ✅ Manejar inscripción / validación de ContestStudent
    var cs = await db.ContestStudents
        .FirstOrDefaultAsync(x => x.ContestId == currentContest.ContestId && x.StudentId == studentId);

    if (cs == null)
    {
        // Nuevo → inscribir con IP actual
        cs = new ContestStudent
        {
            ContestId = currentContest.ContestId,
            StudentId = studentId,
            IP = ip
        };
        db.ContestStudents.Add(cs);
    }
    else
    {
        if (string.IsNullOrEmpty(cs.IP))
        {
            // No tenía IP → asignar ahora
            cs.IP = ip;
        }
        else if (cs.IP != ip)
        {
            // Otro IP → verificar si ya está ocupado por otro estudiante
            bool ipUsed = await db.ContestStudents
                .AnyAsync(x => x.ContestId == currentContest.ContestId && x.IP == ip && x.StudentId != studentId);

            if (ipUsed)
                return Results.BadRequest($"La IP {ip} ya está usada por otro estudiante.");
            else
                return Results.BadRequest($"El estudiante ya estaba registrado con otra IP: {cs.IP}");
        }
    }

    // Obtener todos los archivos del directorio de origen
    string sourceDataSetDir = Path.Combine(problemsRoot, problemId.ToString());
    int resultado = CopyInAndOutFiles(sourceDataSetDir, work);

    if (resultado > 0)
    {
        Console.WriteLine($"Se copiaron {resultado} archivos correctamente.");
    }
    else if (resultado == 0)
    {
        Results.BadRequest("No se encontraron archivos para copiar.");
    }
    else
    {
        Console.WriteLine("Ocurrió un error durante la copia.");
    }

    var fileName = codeFile.FileName;
    var lowered = fileName.ToLowerInvariant();
    var originalPath = Path.Combine(work, fileName);
    
    
    string sourceCode;
    using (var reader = new StreamReader(codeFile.OpenReadStream()))
    {
        sourceCode = await reader.ReadToEndAsync();
    }
    File.WriteAllText(originalPath, sourceCode, Encoding.UTF8);
    Console.WriteLine("Guardando archivo en: " + originalPath);
    try
    {
        if (lowered.EndsWith(".zip")) // Analizar si sigue siendo necesario
        {
            ZipFile.ExtractToDirectory(originalPath, srcDir);
        }
        else if (lowered.EndsWith(".cs"))
        {
            //File.Copy(originalPath, Path.Combine(srcDir, "solucion.cs"), overwrite: true);

            //using var src = File.OpenRead(originalPath);
            //using var dst = File.Create(Path.Combine(srcDir, "solucion.cs"));
            //await src.CopyToAsync(dst);
        }
        else if (lowered.EndsWith(".cpp"))
        {
            //File.Copy(originalPath, Path.Combine(srcDir, "solucion.cpp"), overwrite: true);

            //using var src = File.OpenRead(originalPath);
            //using var dst = File.Create(Path.Combine(srcDir, "solucion.cpp"));
            //await src.CopyToAsync(dst);
        }
        else
        {
            return Results.BadRequest("El archivo 'code' debe ser .cs, cpp o .zip");
        }

        if (inputFile is not null && inputFile.Length > 0)
        {
            var inputPath = Path.Combine(srcDir, "input.txt");
            await using var ifs = File.Create(inputPath);
            {
                await inputFile.CopyToAsync(ifs);
            }
        }
        else if (!string.IsNullOrEmpty(stdinText))
        {
            File.WriteAllText(Path.Combine(srcDir, "input.txt"), stdinText, Encoding.UTF8);
        }

        var volumeInSpec = $"{srcDir}:/home/sandbox/in:ro"; // Solamente, de lectura
        //var volumeInSpec = $"{srcDir}:/home/sandbox/in"; // No, solamente , de lecturar
        var volumeOutSpec = $"{outDir}:/home/sandbox/out";


        //////////////////////////////////////
        // Cambiar permisos en srcDir para permitir lectura al propietario (UID 1000)
        var chmodProcess = new ProcessStartInfo
        {
            FileName = "chmod",
            Arguments = $"-R 777 {srcDir}", // Grant read permissions to the owner
            RedirectStandardOutput = true,
            RedirectStandardError = true,
            UseShellExecute = false
        };

        using (var procChmod = Process.Start(chmodProcess))
        {
            string chmodOutput = await procChmod.StandardOutput.ReadToEndAsync();
            string chmodError = await procChmod.StandardError.ReadToEndAsync();
            procChmod.WaitForExit();

            if (procChmod.ExitCode != 0)
            {
                Console.WriteLine($"Error al cambiar permisos: {chmodError}");
                //return;
            }
        }

        //////////////////////////////////////


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
                    Console.WriteLine($"No se pudo iniciar el proceso chmod para: {fullFilePathValidatorTmp}");
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
        using var proc = Process.Start(psi)!;
        string stdout = await proc.StandardOutput.ReadToEndAsync();
        string stderr = await proc.StandardError.ReadToEndAsync();
        proc.WaitForExit(120_000);

        // 🔹 Recoger salida generada en OUT
        var runLogPath = Path.Combine(outDir, "run.log");
        string runLog = File.Exists(runLogPath) ? await File.ReadAllTextAsync(runLogPath) : "No se generó salida.";


        var finishedAt = DateTimeOffset.UtcNow;

        Console.WriteLine("stdout:" + stdout);
        Console.WriteLine("stderr:" + stderr);
        string buildLog = ExtractSection(stdout, "===BUILD===", "===RUN===");
        string runLogRun = ExtractSection(stdout, "===RUN===", "===SUMMARY===");
        string summary = ExtractSection(stdout, "===SUMMARY===", null);
        string details = ExtractSection(summary, "DETAILS:", null).Replace("DETAILS:", "");

        string filesSection = ExtractSection(stdout, "===FILES===", "===END-FILES===");
        Console.WriteLine("FILES:\n" + filesSection);

        string build = TryMatch(summary, @"build:(ok|error)")
            ?? (buildLog.Contains("Build succeeded.", StringComparison.OrdinalIgnoreCase) ? "ok" : "error");
        string run = TryMatch(summary, @"run:(ok|error)")
            ?? (Regex.IsMatch(runLog, @"timed out|Killed|Unhandled|Exception", RegexOptions.IgnoreCase) ? "error" : "ok");
        if (details.Length == 0)
        {
            run = "error";
            const string ERROR_TIMEOUT = "Error en tiempo de ejecucion. Posible timeout u otro error inesperado";
            Console.WriteLine(ERROR_TIMEOUT);
            Console.WriteLine();
            summary = summary + " " + ERROR_TIMEOUT;
        }

        string? time = TryMatch(runLog, @"TIME=([^\r\n]+)", 1);
        string? memKb = TryMatch(runLog, @"MEM=(\d+)KB", 1);

        // 🔹 Comparación con el esperado
        string expected = form["expected"].ToString();
        //bool isCorrect = Normalize(expected) == Normalize(runLog);

        var m = Regex.Match(summary, @"run:ok", RegexOptions.IgnoreCase);
        bool isCorrect = m.Success;


        var submission = new Submission
        {
            StudentId = studentId,
            ProblemId = form["problemId"].ToString(),
            SourceCode = sourceCode,
            OutputExpected = expected,
            OutputActual = runLog,
            IsCorrect = isCorrect,
            IP = ip
        };

        db.Submissions.Add(submission);
        await db.SaveChangesAsync();

        return Results.Json(new
        {
            id,
            submission.SubmissionId,
            submission.IsCorrect,
            build,
            run,
            time,
            memKb,
            buildLog,
            runLog,
            stderrRaw = stderr,
            summary = summary
        }, new JsonSerializerOptions { WriteIndented = true });
    }
    finally
    {
        if (!keep)
        {
            try { Directory.Delete(work, recursive: true); } catch { }
        }
    }
}).DisableAntiforgery();


/// <summary>
/// Copia todos los archivos de los subdirectorios IN y OUT al directorio destino
/// </summary>
/// <param name="sourceDirectory">Directorio fuente que contiene los subdirectorios IN y PUT</param>
/// <param name="targetDirectory">Directorio destino donde se copiarán los archivos</param>
/// <param name="overwrite">Indica si se deben sobrescribir archivos existentes (opcional, default=true)</param>
/// <returns>Número total de archivos copiados</returns>
static int CopyInAndOutFiles(string sourceDirectory, string targetDirectory, bool overwrite = true)
{
    int filesCopied = 0;

    try
    {
        // Validar directorio fuente
        if (!Directory.Exists(sourceDirectory))
            throw new DirectoryNotFoundException($"El directorio fuente no existe: {sourceDirectory}");

        // Crear directorio destino si no existe
        if (!Directory.Exists(targetDirectory))
            Directory.CreateDirectory(targetDirectory);

        // Subdirectorios a copiar
        const string IN = "IN";
        const string OUT = "OUT";
        string[] subdirectories = { IN, OUT };

        foreach (string subdir in subdirectories)
        {
            string sourceSubDir = Path.Combine(sourceDirectory, subdir);
            string targetDirectorySubDir = Path.Combine(targetDirectory, subdir);
            // Crea el subdirectorio objetivo, si no existe
            if (!Directory.Exists(targetDirectorySubDir))
                Directory.CreateDirectory(targetDirectorySubDir);

            if (Directory.Exists(sourceSubDir))
            {
                // Copiar cada archivo del subdirectorio
                foreach (string sourceFile in Directory.GetFiles(sourceSubDir))
                {
                    string fileName = Path.GetFileName(sourceFile);
                    string targetFile =
                        Path.Combine(targetDirectorySubDir, fileName);

                    File.Copy(sourceFile, targetFile, overwrite);
                    filesCopied++;
                }
            }
        }

        return filesCopied;
    }
    catch (Exception ex)
    {
        Console.WriteLine($"Error al copiar archivos: {ex.Message}");
        return -1; // Retorna -1 para indicar error
    }
}

// === HELPERS ===
static string ExtractSection(string text, string startMarker, string? nextMarker)
{
    int s = text.IndexOf(startMarker, StringComparison.Ordinal);
    if (s < 0) return "";
    s += startMarker.Length;
    int e = nextMarker is null ? text.Length : text.IndexOf(nextMarker, s, StringComparison.Ordinal);
    if (e < 0) e = text.Length;
    return text.Substring(s, e - s).Trim();
}

static string? TryMatch(string text, string pattern, int group = 0)
{
    var m = Regex.Match(text, pattern, RegexOptions.IgnoreCase);
    if (!m.Success) return null;
    return m.Groups.Count > group ? m.Groups[group].Value : m.Value;
}

//static string Normalize(string s) => s.Replace("\r\n", "\n").Trim();


// NOTA. - Si el MapGet("/students", ...) quedó después de "app.Run();", no quedar[a registrado y no se ejecutara.
// Todo lo que esté después de app.Run(); no se ejecuta.
app.MapGet("/students", async (AppDbContext db) =>
{
    var estudiantes = await db.Students
        .OrderBy(s => s.Name)
        .Select(s => new { name = s.Name, studentId = s.StudentId })
        .ToListAsync();

    return Results.Json(estudiantes);
});

// Obtiene todas las preguntas de un contest
app.MapGet("/contest/questions", async (AppDbContext db) =>
{
    // Validar que haya un contest activo
    var contest = await db.Contests
        .OrderByDescending(c => c.Date)
        .ThenByDescending(c => c.ContestId)
        .FirstOrDefaultAsync();

    if (contest == null)
        return Results.BadRequest("No hay contest activo");

    var preguntas = await db.Questions
        .Where(q => q.ContestId == contest.ContestId)
        .OrderBy(q => q.QuestionId)
        .Select(q => new
        {
            id = q.QuestionId,
            titulo = q.Review
        })
        .ToListAsync();

    return Results.Json(preguntas);
});


// Obtiene el enunciado de una determinada pregunta
app.MapGet("/questions/{id}/desc", (int id) =>
{
    try
    {
        string baseDir = PathDirectories.PROBLEMS_PATH;
        string path = Directory.GetFiles(Path.Combine(baseDir, id.ToString()), "*.docx").FirstOrDefault();
        if (string.IsNullOrEmpty(path) || !System.IO.File.Exists(path))
            return Results.NotFound($"No existe el archivo para la pregunta {id}");

        using var doc = DocX.Load(path);

        // Extraer párrafos respetando saltos de línea
        var plainText = string.Join("\n", doc.Paragraphs.Select(p => p.Text));

        return Results.Json(new { id, text = plainText });
    }
    catch (Exception ex)
    {
        return Results.BadRequest(ex.Message);
    }
});


// Listar datasets disponibles de un problema
app.MapGet("/problems/{id}/inputs", (int id) =>
{
    string dir = Path.Combine(PathDirectories.PROBLEMS_PATH, id.ToString(), "IN");
    if (!Directory.Exists(dir))
        return Results.NotFound("No hay inputs para este problema");

    var archivos = Directory.GetFiles(dir, "*.txt")
                            .Select(f => Path.GetFileName(f))
                            .OrderBy(f => Path.GetFileName(f))
                            .ToList();

    return Results.Json(archivos);
});

// Obtener contenido de un dataset
app.MapGet("/problems/{id}/input/{archivo}", (int id, string archivo) =>
{
    string dir = Path.Combine(PathDirectories.PROBLEMS_PATH, id.ToString(), "IN");
    string path = Path.Combine(dir, archivo);

    if (!System.IO.File.Exists(path))
        return Results.NotFound("Archivo no encontrado");

    string contenido = System.IO.File.ReadAllText(path);
    return Results.Text(contenido, "text/plain");
});

// Estructura de directorios
app.MapGet("/utils/tree", () =>
{
    string root = Path.Combine(PathDirectories.UTILS_PATH);
    var tree = BuildTree(root, root);
    return Results.Json(tree);
});

// Contenido de archivo
app.MapGet("/utils/file/{*path}", (string path) =>
{
    string fullPath = Path.Combine(PathDirectories.UTILS_PATH, path);
    if (!System.IO.File.Exists(fullPath)) return Results.NotFound();
    return Results.Text(System.IO.File.ReadAllText(fullPath), "text/plain");
});

// Helper recursivo
static object BuildTree(string root, string dir)
{
    return new
    {
        name = Path.GetFileName(dir),
        type = "dir",
        children = Directory.GetDirectories(dir)
            .Select(d => BuildTree(root, d))
            .Concat(Directory.GetFiles(dir).Select(f => new
            {
                name = Path.GetFileName(f),
                type = "file",
                path = Path.GetRelativePath(root, f).Replace("\\", "/")
            }))
            .ToList()
    };
}


// NOTA. - Mejor, si va a al final
app.Run();


class PathDirectories
{
    public static string DB_PATH = Environment.GetEnvironmentVariable("DB_PATH")
                       ?? "/home/vboxuser/VirtualJudge/CsJudgeApi/submissions.db";
    public static string PROBLEMS_PATH = Environment.GetEnvironmentVariable("PROBLEMS_PATH")
                       ?? "/home/vboxuser/VirtualJudge/problems";
    public static string UTILS_PATH =  Environment.GetEnvironmentVariable("UTILS_PATH") 
                         ?? "/home/vboxuser/VirtualJudge/Utilitarios";
}
