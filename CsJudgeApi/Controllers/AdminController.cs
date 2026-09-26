using CsJudgeApi.Data;
using CsJudgeApi.Services;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace CsJudgeApi.Controllers;

[ApiController]
[Route("api/[controller]")]
public class AdminController : ControllerBase
{
    private readonly StudentImportService _studentImportService;
    private readonly ValidatorPathRestoreService _validatorRestoreService;
    private readonly AppDbContext _context;
    private readonly ILogger<AdminController> _logger;

    public AdminController(
        StudentImportService studentImportService,
        ValidatorPathRestoreService validatorRestoreService,
        AppDbContext context,
        ILogger<AdminController> logger)
    {
        _studentImportService = studentImportService;
        _validatorRestoreService = validatorRestoreService;
        _context = context;
        _logger = logger;
    }

    /// <summary>
    /// Importa estudiantes desde un archivo CSV
    /// POST /api/admin/import-students
    /// Parámetros:
    ///   - csvFile: Archivo CSV con columnas "Documento Identidad" y "Nombre Estudiante"
    ///   - semesterId: ID del semestre (1=2025-2, 2=2026-2)
    ///   - group: Número de grupo (1=Paralelo 1, 2=Paralelo 2)
    ///   - parallelName: Nombre del paralelo (ej: "Paralelo 1")
    /// </summary>
    [HttpPost("import-students")]
    public async Task<IActionResult> ImportStudents(
        [FromForm] IFormFile csvFile,
        [FromForm] int semesterId,
        [FromForm] int group,
        [FromForm] string parallelName)
    {
        if (csvFile == null || csvFile.Length == 0)
        {
            return BadRequest(new
            {
                error = "Archivo CSV no proporcionado",
                success = false
            });
        }

        if (string.IsNullOrEmpty(parallelName))
        {
            return BadRequest(new
            {
                error = "ParallelName es requerido",
                success = false
            });
        }

        if (semesterId <= 0 || group <= 0)
        {
            return BadRequest(new
            {
                error = "semesterId y group deben ser mayores a 0",
                success = false
            });
        }

        var tempPath = Path.Combine(Path.GetTempPath(), csvFile.FileName);

        try
        {
            _logger.LogInformation($"Iniciando importación: {csvFile.FileName}, Semestre: {semesterId}, Grupo: {group}");

            using (var stream = new FileStream(tempPath, FileMode.Create))
            {
                await csvFile.CopyToAsync(stream);
            }

            var result = await _studentImportService.ImportStudentsFromCsv(
                tempPath, semesterId, group, parallelName);

            if (result.HasError)
            {
                return BadRequest(new
                {
                    success = false,
                    message = result.ErrorMessage,
                    imported = result.SuccessCount,
                    failed = result.FailureCount,
                    skipped = result.SkippedCount,
                    total = result.TotalProcessed,
                    errors = result.Errors
                });
            }

            _logger.LogInformation($"Importación completada: {result.SuccessCount} exitosos, {result.FailureCount} fallidos, {result.SkippedCount} omitidos");

            return Ok(new
            {
                success = true,
                message = $"Importación completada",
                imported = result.SuccessCount,
                failed = result.FailureCount,
                skipped = result.SkippedCount,
                total = result.TotalProcessed,
                errors = result.Errors.Count > 0 ? result.Errors : null
            });
        }
        catch (Exception ex)
        {
            _logger.LogError($"Error en importación: {ex.Message}\n{ex.StackTrace}");
            return StatusCode(500, new
            {
                success = false,
                error = ex.Message
            });
        }
        finally
        {
            if (System.IO.File.Exists(tempPath))
            {
                System.IO.File.Delete(tempPath);
            }
        }
    }

    /// <summary>
    /// Obtiene todas los semestres registrados
    /// GET /api/admin/semesters
    /// </summary>
    [HttpGet("semesters")]
    public async Task<ActionResult<IEnumerable<object>>> GetSemesters()
    {
        var semesters = await _context.Semesters
            .Select(s => new
            {
                s.SemesterId,
                s.SemesterCode,
                s.Name,
                s.StartDate,
                s.EndDate,
                s.IsActive
            })
            .ToListAsync();

        return Ok(semesters);
    }

    /// <summary>
    /// Obtiene estadísticas de estudiantes por semestre/grupo
    /// GET /api/admin/statistics
    /// </summary>
    [HttpGet("statistics")]
    public async Task<ActionResult<object>> GetStatistics()
    {
        var statistics = await _context.StudentSemesters
            .GroupBy(ss => new { ss.SemesterId, ss.Semester!.Name, ss.Group })
            .Select(g => new
            {
                SemesterId = g.Key.SemesterId,
                SemesterName = g.Key.Name,
                Group = g.Key.Group,
                StudentCount = g.Count()
            })
            .OrderByDescending(s => s.SemesterId)
            .ThenBy(s => s.Group)
            .ToListAsync();

        return Ok(statistics);
    }

    /// <summary>
    /// Restaura rutas de validadores desde un backup de la BD
    /// Verifica que los archivos existan en el filesystem
    /// POST /api/admin/restore-validator-paths
    /// Body: { "backupPath": "submissions.db.bkp.2026-08-17" }
    /// Si backupPath es null, usa por defecto: "submissions.db.bkp.2026-08-17"
    /// </summary>
    [HttpPost("restore-validator-paths")]
    public async Task<IActionResult> RestoreValidatorPaths(
        [FromBody] RestoreRequest request)
    {
        try
        {
            _logger.LogInformation($"Iniciando restauración de validadores desde backup: {request.BackupPath ?? "submissions.db.bkp.2026-08-17"}");

            var result = await _validatorRestoreService.RestoreValidatorPathsFromBackup(
                request.BackupPath ?? "submissions.db.bkp.2026-08-17");

            return Ok(new 
            { 
                success = result.Success,
                restored = result.RestoredCount,
                failed = result.FailedCount,
                savedChanges = result.SavedChanges,
                message = result.ErrorMessage ?? "Restauración completada exitosamente"
            });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error en restauración de validadores");
            return StatusCode(500, new
            {
                success = false,
                error = ex.Message,
                message = "Error al restaurar validadores"
            });
        }
    }
}
