using System.IO.Compression;
using System.Text;
using System.Text.Json;
using System.Text.RegularExpressions;
using System.Diagnostics;
using Microsoft.EntityFrameworkCore;
using CsJudgeApi;
using CsJudgeApi.Data;
using CsJudgeApi.Models;
using Xceed.Words.NET;
using CsJudgeApi.Models.Enums; // ⚡ DocX. Para instalar, hacer => dotnet add package DocX --version 1.0.0


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


// BOOKMARK: Configuración de base de datos y servidor
// EF Core con SQLite
builder.Services.AddDbContext<AppDbContext>(options =>
    options.UseSqlite($"Data Source={PathDirectories.DB_PATH}"));

builder.WebHost.ConfigureKestrel(o =>
{
    o.Limits.MaxRequestBodySize = 20 * 1024 * 1024; // 20MB
    o.Limits.MaxConcurrentConnections = 30; // Máximo 30 conexiones simultáneas
    o.Limits.MaxConcurrentUpgradedConnections = 30; // Para WebSockets/HTTP2
});


builder.WebHost.UseUrls("http://0.0.0.0:5000");


var app = builder.Build();
app.UseCors("AllowAll");

// Habilitar archivos estáticos (sirve index.html desde wwwroot/)
app.UseDefaultFiles();
app.UseStaticFiles();


// BOOKMARK: Sistema de throttling y variables globales
// Diccionario para throttling por IP (en producción usar Redis/MemoryCache)
var lastSubmissionByIp = new Dictionary<string, DateTime>();
var submissionCooldownSeconds = 5; // 5 segundos entre submissions del mismo IP

Contest? currentContest = null;
// Aseguramos DB creada
using (var scope = app.Services.CreateScope())
{
    var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();
    db.Database.EnsureCreated();

    // Buscar valor configurado
    var configContestId = db.Configurations
        .Where(c => c.Key == "CurrentContestId")
        .Select(c => c.Value)
        .FirstOrDefault();

    // Intentar parsear el valor
    if (int.TryParse(configContestId, out int contestId))
    {
        currentContest = db.Contests.FirstOrDefault(c => c.ContestId == contestId);
        if (currentContest == null)
        {
            Console.WriteLine($"No se encontró el contest con ContestId={contestId} configurado como CurrentContestId. Se va a utilizar el Const con mayor valor en el campo ContestId.");
        }
    }

    // Si no existe o no es válido, usar el último
    if (currentContest == null)
    {
        currentContest = db.Contests
            .OrderByDescending(c => c.ContestId)
            .FirstOrDefault();
    }

    Console.WriteLine("ContestId=" + currentContest?.ContestId + " Date=" + currentContest?.Date);
}


// NOTA. - Flujo resumido (con DbContext)
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

// BOOKMARK: Endpoints de salud y estado del servicio
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

/// <summary>
/// Obtiene la IP del cliente, considerando proxies
/// </summary>
/// <param name="req">La solicitud HTTP</param>
/// <returns>La IP del cliente</returns>
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


// BOOKMARK: Endpoint principal - Compilar y ejecutar código
// === ENDPOINT COMPILAR Y EJECUTAR ===
app.MapPost("/compile-run", async (HttpRequest req, AppDbContext db) =>
{
    if (!req.HasFormContentType)
        return Results.BadRequest("Envía un formulario multipart con el archivo 'code' (.cs o .zip).");

    // ✅ Obtener la IP del cliente
    var ip = GetClientIp(req);

    // 🚦 Throttling por IP
    lock (lastSubmissionByIp)
    {
        if (lastSubmissionByIp.TryGetValue(ip, out var lastTime))
        {
            var timeSinceLastSubmission = DateTime.UtcNow - lastTime;
            if (timeSinceLastSubmission.TotalSeconds < submissionCooldownSeconds)
            {
                var waitTime = submissionCooldownSeconds - (int)timeSinceLastSubmission.TotalSeconds;
                return Results.BadRequest($"Debes esperar {waitTime} segundos antes de enviar otra submission.");
            }
        }
        lastSubmissionByIp[ip] = DateTime.UtcNow;
    }

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
    using (var procA = Process.Start(chmodA))
    {
        procA?.WaitForExit();
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
        if (procSrcDir == null) return Results.BadRequest("No se pudo iniciar el proceso chmod.");
        
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

    if(question.TimeLimitSeconds != null && question.TimeLimitSeconds != timeLimit)
    {
        timeLimit = question.TimeLimitSeconds.Value;
        Console.WriteLine($"Ajustando timeLimit a {timeLimit} segundos, según la configuración de la pregunta.");
    }
    
    // ✅ Validar que el estudiante exista
    var student = db.Students.FirstOrDefault(s => s.StudentId == studentId);
    if (student == null)
        return Results.BadRequest($"El estudiante con numero de registro {studentId} no existe");

    Console.WriteLine("Request enviado por el estudiante " + student.Name);

    // Verifica que la ip, no este siendo utilizada por otro estudiante, en el mismo Contest
    var (flowControl, value) = await IpIsNotUsed(db, currentContest, student, ip);
    if (!flowControl)
    {
        return value;
    }

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
            // 🔍 DEBUG: Comparar IPs exactamente
            Console.WriteLine($"🔍 DEBUG COMPILE-RUN:");
            Console.WriteLine($"   Estudiante: {studentId} ({student.Name})");
            Console.WriteLine($"   IP registrada: '{cs.IP}' (Length: {cs.IP.Length})");
            Console.WriteLine($"   IP actual: '{ip}' (Length: {ip.Length})");
            Console.WriteLine($"   ¿Son iguales? {cs.IP == ip}");
            
            // Otro IP → El estudiante ya estaba registrado con otra ip, en el mismo Contest
            return Results.BadRequest($"El estudiante ya estaba registrado con otra IP: {cs.IP}");
        }
        else
        {
            // 🔍 DEBUG: IP coincide
            Console.WriteLine($"✅ IP COINCIDE - Estudiante {studentId} desde IP {ip}");
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
            if (procChmod == null) return Results.BadRequest("No se pudo iniciar el proceso chmod.");

            string chmodOutput = await procChmod.StandardOutput.ReadToEndAsync();
            string chmodError = await procChmod.StandardError.ReadToEndAsync();
            procChmod.WaitForExit();

            if (procChmod.ExitCode != 0)
            {
                return Results.BadRequest($"Error al cambiar permisos: {chmodError}");
            }
        }

        // BOOKMARK: Selección del executor (Docker vs Nsjail)
        //////////////////////////////////////

        string stdout, stderr;
        CsJudgeApi.Models.Enums.EContainerType containerType = GetContainerType(db);
        switch(containerType)
        {
            case EContainerType.Docker:
                (bool flowControlDocker, IResult valueDocker) =
                    CsJudgeApi.DockerExecutor.Execute(language, timeLimit, validatorDir, problemsRoot, problemId, question, volumeInSpec, volumeOutSpec, out stdout, out stderr, srcDir);
                if (!flowControlDocker)
                {
                    return valueDocker;
                }
                break;
            case EContainerType.nsjail:
                (bool flowControlSandbox, IResult valueSandbox) =
                    CsJudgeApi.NsjailExecutor.Execute(language, timeLimit, validatorDir, problemsRoot, problemId, question, volumeInSpec, volumeOutSpec, out stdout, out stderr, srcDir);
                if (!flowControlSandbox)
                {
                    return valueSandbox;
                }
                break;
            default:
                return Results.BadRequest("Tipo de contenedor no soportado.");  
        }
    

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


EContainerType GetContainerType(AppDbContext db)
{
    var configuration = db.Configurations.FirstOrDefault(c => c.Key == "ContainerType");
    if (configuration != null && Enum.TryParse<EContainerType>(configuration.Value, out var containerType))
    {
        Console.WriteLine("ContainerType obtenido de configuración: " + containerType);
        return containerType;
    }
    return EContainerType.Docker; // Valor por defecto
}


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


// BOOKMARK: API endpoints para contests y estudiantes
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
    var contest = currentContest;

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
        string path = Directory.GetFiles(Path.Combine(baseDir, id.ToString()), "*.docx").FirstOrDefault() ?? "";
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


/// <summary>
/// Verifica si la configuración DisableIpCheck está activada en la base de datos.
/// </summary>
/// <param name="db">Contexto de la base de datos.</param>
/// <returns>true o false, dependiendo de si está habilitada o no.</returns
static bool IsIpCheckDisable(AppDbContext db)
{
    // BOOKMARK: Verificar si la configuración DisableIpCheck está activada
    Console.WriteLine(db.Configurations.Any(c => c.Key == "DisableIpCheck") + "--" + db.Configurations.FirstOrDefault(c => c.Key == "DisableIpCheck")?.Value);
    if (db.Configurations.Any(c => c.Key == "DisableIpCheck" && (c.Value == "true" || c.Value == "1")))
    {
        return true;
    }

    return false;
}

/// <summary>
/// Verifica si la IP ya está siendo usada por otro estudiante, en el mismo Contest.
/// </summary>
/// <param name="db">Contexto de la base de datos.</param>
/// <param name="currentContest">El contest actual.</param>
/// <param name="student">El estudiante que intenta registrarse.</param>
/// <param name="ip">La IP del estudiante.</param>
/// <returns>Tupla con flowControl (bool) y value (IResult?)</returns>
static async Task<(bool flowControl, IResult? value)> IpIsNotUsed(AppDbContext db, Contest currentContest, Student student, string ip)
{
    if (!IsIpCheckDisable(db))
    {
        // Si no se deshabilto el control de IPs, verificar que la IP no esté siendo usada por otro 
        //estudiante
        bool ipUsed = await db.ContestStudents
            .AnyAsync(x => x.ContestId == currentContest.ContestId && x.IP == ip && student.StudentId != x.StudentId);

        if (ipUsed)
        {
            Console.WriteLine($"❌ BLOQUEADO: IP {ip} ya está usada por otro estudiante");
            return (flowControl: false, value: Results.BadRequest(new { error = $"La IP {ip} ya está siendo usada por otro estudiante en este Contest." }));
        }
    }

    return (flowControl: true, value: null);
}


if (currentContest is null)
    throw new Exception("No hay contest activo");
    
// BOOKMARK: API para dashboard y estadísticas
// Endpoint rápido de Dashboard
app.MapGet("/api/dashboard", async (AppDbContext db) =>
{
    var result = await (
        from su in db.Submissions
        join st in db.Students on su.StudentId equals st.StudentId
        join q in db.Questions on su.ProblemId equals q.QuestionId.ToString()
        join c in db.Contests on q.ContestId equals c.ContestId
        where c.ContestId == currentContest.ContestId && st.StudentId != 123
        orderby su.CreatedAt descending
        select new
        {
            st.Name,
            su.ProblemId,
            su.SubmissionId,
            su.IsCorrect,
            su.CreatedAt
        }
    ).ToListAsync();

    return Results.Json(result);
});

// Endpoint de estado del contest (submissions recientes, con detalles)
app.MapGet("/api/contest-status", async (AppDbContext db) =>
{
    
    if (currentContest  == null)
        return Results.BadRequest("No hay contest activo");

    // Submissions con join
    var data = await (
        from su in db.Submissions
        join st in db.Students on su.StudentId equals st.StudentId
        join q in db.Questions on su.ProblemId equals q.QuestionId.ToString()
        where q.ContestId == currentContest.ContestId
        orderby su.CreatedAt descending
        select new
        {
            ContestId = currentContest.ContestId,
            ContestDate = currentContest.Date,
            Student = st.Name,
            ProblemId = su.ProblemId,
            su.IsCorrect,
            su.CreatedAt
        }
    ).ToListAsync();

    return Results.Json(data);
});

// === Endpoint para registrar login/actividad de estudiante ===
app.MapPost("/api/student-login", async (HttpRequest request, AppDbContext db) =>
{
    try
    {
        // Leer el JSON del body
        using var reader = new StreamReader(request.Body);
        var json = await reader.ReadToEndAsync();
        
        var loginData = JsonSerializer.Deserialize<JsonElement>(json);
        
        // Extraer datos del JSON
        var studentIdStr = loginData.GetProperty("studentId").GetString();
        var timestamp = loginData.GetProperty("timestamp").GetString();
        var action = loginData.GetProperty("action").GetString();
        
        if (!long.TryParse(studentIdStr, out long studentId))
        {
            return Results.BadRequest(new { error = "StudentId inválido" });
        }

        // Verificar que el estudiante existe
        var student = await db.Students.FirstOrDefaultAsync(s => s.StudentId == studentId);
        if (student == null)
        {
            return Results.NotFound(new { error = $"Estudiante {studentId} no encontrado" });
        }

        // Verificar que hay un contest activo
        if (currentContest == null)
        {
            return Results.BadRequest(new { error = "No hay contest activo" });
        }

        // Obtener IP del cliente (usando la misma función que compile-run)
        var ip = GetClientIp(request);
        
        // Log en consola del servidor
        Console.WriteLine($"🔐 LOGIN: Estudiante {studentId} ({student.Name}) - {action} - {timestamp} - IP: {ip}");

        // ✅ Manejar inscripción/validación de ContestStudent (lógica estricta anti-fraude)
        var cs = await db.ContestStudents
            .FirstOrDefaultAsync(x => x.ContestId == currentContest.ContestId && x.StudentId == studentId);

        bool isNewRegistration = false;

        // Verifica que la ip, no este siendo utilizada por otro estudiante, en el mismo Contest
        var (flowControl, value) = await IpIsNotUsed(db, currentContest, student, ip);
        if (!flowControl)
        {
            return value;
        }

        if (cs == null)
        {
            // Nuevo → inscribir con IP actual
            cs = new ContestStudent
            {
                ContestId = currentContest.ContestId,
                StudentId = studentId,
                IP = ip,
                DateParticipation = DateTime.Now
            };
            db.ContestStudents.Add(cs);
            isNewRegistration = true;
            Console.WriteLine($"✅ NUEVO: Inscribiendo estudiante {studentId} ({student.Name}) en contest {currentContest.ContestId} desde IP {ip}");
        }
        else
        {
            bool isIpCheckDisable = IsIpCheckDisable(db);

            // BOOKMARK: Ya existe → validar IP estrictamente
            if (string.IsNullOrEmpty(cs.IP))
            {
                // No tenía IP → asignar ahora
                cs.IP = ip;
                cs.DateParticipation = DateTime.Now;
                Console.WriteLine($"🔄 IP ASIGNADA: Estudiante {studentId} ({student.Name}) ahora desde IP {ip}");
            }
            else if (cs.IP != ip && !isIpCheckDisable)
            {
                // ❌ DIFERENTE IP → ERROR ESTRICTO (anti-fraude)
                Console.WriteLine($"❌ BLOQUEADO: Estudiante {studentId} ({student.Name}) intentó acceder desde IP {ip}, pero ya está registrado con IP {cs.IP}");
                return Results.BadRequest(new { 
                    error = $"El estudiante {student.Name} ya está registrado en este contest desde otra IP ({cs.IP}). No se permite el acceso desde múltiples ubicaciones.",
                    registeredIp = cs.IP,
                    currentIp = ip
                });
            }
            else
            {
                // Misma IP → solo actualizar fecha de actividad
                cs.DateParticipation = DateTime.Now;
                Console.WriteLine($"🔄 ACTIVIDAD: Estudiante {studentId} ({student.Name}) conectado desde IP {ip}");
            }
        }

        // Guardar cambios en la base de datos
        await db.SaveChangesAsync();

        return Results.Ok(new 
        { 
            success = true, 
            studentId = studentId,
            studentName = student.Name,
            contestId = currentContest.ContestId,
            contestName = $"Contest {currentContest.ContestId}",
            action = action,
            timestamp = timestamp,
            ip = ip,
            isNewRegistration = isNewRegistration,
            dateParticipation = cs.DateParticipation,
            message = isNewRegistration 
                ? $"Estudiante {student.Name} inscrito en Contest {currentContest.ContestId}" 
                : $"Actividad registrada para {student.Name} en Contest {currentContest.ContestId}"
        });
    }
    catch (Exception ex)
    {
        Console.WriteLine($"❌ Error en /api/student-login: {ex.Message}");
        return Results.StatusCode(500);
    }
});


// NOTA. - Mejor, si va a al final
app.Run();
