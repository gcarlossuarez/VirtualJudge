using System;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using CsJudgeApi.Data;
using Microsoft.Data.Sqlite;
using Microsoft.EntityFrameworkCore;

namespace CsJudgeApi.Services;

/// <summary>
/// Servicio para restaurar rutas de validadores desde un backup de la BD
/// verificando que los archivos existan en el filesystem
/// </summary>
public class ValidatorPathRestoreService
{
    private readonly AppDbContext _context;
    private readonly ILogger<ValidatorPathRestoreService> _logger;

    public ValidatorPathRestoreService(AppDbContext context, ILogger<ValidatorPathRestoreService> logger)
    {
        _context = context;
        _logger = logger;
    }

    /// <summary>
    /// Restaura rutas de validadores desde backup
    /// verificando que los archivos existan en el filesystem
    /// </summary>
    public async Task<RestoreResult> RestoreValidatorPathsFromBackup(string backupPath)
    {
        var result = new RestoreResult();

        try
        {
            // Verificar que el backup existe
            if (!File.Exists(backupPath))
            {
                result.Success = false;
                result.ErrorMessage = $"Archivo de backup no encontrado: {backupPath}";
                _logger.LogError(result.ErrorMessage);
                return result;
            }

            // 1️⃣ Abrir backup con SQLite directo
            var backupDb = new SqliteConnection($"Data Source={backupPath}");
            await backupDb.OpenAsync();

            var cmd = backupDb.CreateCommand();
            cmd.CommandText = "SELECT QuestionId, FullPathValidatorSourceCode FROM Questions WHERE FullPathValidatorSourceCode != '' AND FullPathValidatorSourceCode IS NOT NULL";
            
            var reader = await cmd.ExecuteReaderAsync();

            while (await reader.ReadAsync())
            {
                try
                {
                    int qid = reader.GetInt32(0);
                    string path = reader.GetString(1);

                    // 2️⃣ VERIFICAR QUE EL ARCHIVO EXISTE
                    if (File.Exists(path))
                    {
                        var question = await _context.Questions
                            .FirstOrDefaultAsync(q => q.QuestionId == qid);

                        if (question != null)
                        {
                            if (string.IsNullOrEmpty(question.FullPathValidatorSourceCode))
                            {
                                question.FullPathValidatorSourceCode = path;
                                result.RestoredCount++;
                                _logger.LogInformation($"✓ Q{qid}: Restaurada ruta {path}");
                            }
                            else if (question.FullPathValidatorSourceCode != path)
                            {
                                // Si la ruta es diferente, actualizar si el nuevo archivo existe
                                _logger.LogWarning($"Q{qid}: Ruta diferente. Actual: {question.FullPathValidatorSourceCode}, Backup: {path}");
                            }
                        }
                    }
                    else
                    {
                        result.FailedCount++;
                        _logger.LogWarning($"✗ Q{qid}: Archivo NO existe en filesystem: {path}");
                    }
                }
                catch (Exception ex)
                {
                    result.FailedCount++;
                    _logger.LogError(ex, "Error procesando pregunta");
                }
            }

            await backupDb.CloseAsync();

            // 3️⃣ Guardar cambios
            int saved = await _context.SaveChangesAsync();
            result.Success = true;
            result.SavedChanges = saved;

            _logger.LogInformation($"✓ Restauración completada: {result.RestoredCount} preguntas restauradas, {result.FailedCount} fallos, {saved} cambios guardados");
        }
        catch (Exception ex)
        {
            result.Success = false;
            result.ErrorMessage = ex.Message;
            _logger.LogError(ex, "Error restaurando validadores desde backup");
        }

        return result;
    }
}

/// <summary>
/// Resultado de la restauración de validadores
/// </summary>
public class RestoreResult
{
    public bool Success { get; set; }
    public int RestoredCount { get; set; }
    public int FailedCount { get; set; }
    public int SavedChanges { get; set; }
    public string ErrorMessage { get; set; } = "";
}

/// <summary>
/// Request para restaurar validadores
/// </summary>
public class RestoreRequest
{
    public string? BackupPath { get; set; }
}
