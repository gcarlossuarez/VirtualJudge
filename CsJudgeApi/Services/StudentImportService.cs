using CsJudgeApi.Data;
using CsJudgeApi.Models;
using System.Globalization;
using CsvHelper;
using CsvHelper.Configuration;
using Microsoft.EntityFrameworkCore;

namespace CsJudgeApi.Services;

public class StudentImportService
{
    private readonly AppDbContext _context;
    private readonly ILogger<StudentImportService> _logger;

    public StudentImportService(AppDbContext context, ILogger<StudentImportService> logger)
    {
        _context = context;
        _logger = logger;
    }

    /// <summary>
    /// Importa estudiantes desde un archivo CSV y los asocia a un semestre/grupo
    /// </summary>
    public async Task<ImportResult> ImportStudentsFromCsv(
        string csvFilePath,
        int semesterId,
        int group,
        string parallelName)
    {
        var result = new ImportResult();

        try
        {
            // Verificar que el archivo existe
            if (!File.Exists(csvFilePath))
            {
                result.HasError = true;
                result.ErrorMessage = $"Archivo no encontrado: {csvFilePath}";
                return result;
            }

            // Verificar que el semestre existe
            var semester = _context.Semesters
                .FirstOrDefault(s => s.SemesterId == semesterId);

            if (semester == null)
            {
                result.HasError = true;
                result.ErrorMessage = $"Semestre no encontrado: {semesterId}";
                return result;
            }

            using (var reader = new StreamReader(csvFilePath, System.Text.Encoding.UTF8))
            using (var csv = new CsvReader(reader, CultureInfo.InvariantCulture))
            {
                // Configurar mapeo de columnas
                csv.Context.RegisterClassMap<StudentCsvMap>();
                
                var records = csv.GetRecords<StudentCsvDto>().ToList();

                _logger.LogInformation($"Leyendo {records.Count} registros del CSV");

                foreach (var record in records)
                {
                    // Validar que StudentId sea válido
                    if (record.StudentId <= 0)
                    {
                        result.Errors.Add($"Estudiante {record.Name}: StudentId inválido ({record.StudentId})");
                        result.FailureCount++;
                        continue;
                    }

                    try
                    {
                        // Verificar si estudiante existe
                        var existingStudent = await _context.Students
                            .FirstOrDefaultAsync(s => s.StudentId == record.StudentId);

                        if (existingStudent == null)
                        {
                            existingStudent = new Student
                            {
                                StudentId = record.StudentId,
                                Name = record.Name
                            };
                            _context.Students.Add(existingStudent);
                            await _context.SaveChangesAsync();
                            _logger.LogInformation($"Nuevo estudiante creado: {record.StudentId} - {record.Name}");
                        }

                        // Verificar si ya existe la relación StudentSemester
                        var exists = await _context.StudentSemesters
                            .AnyAsync(ss => ss.StudentId == record.StudentId && ss.SemesterId == semesterId);

                        if (!exists)
                        {
                            var studentSemester = new StudentSemester
                            {
                                StudentId = record.StudentId,
                                SemesterId = semesterId,
                                Group = group,
                                ParallelName = parallelName,
                                EnrollmentDate = DateTime.Now
                            };

                            _context.StudentSemesters.Add(studentSemester);
                            result.SuccessCount++;
                            _logger.LogInformation($"Inscrito: {record.StudentId} - {record.Name} en {parallelName}");
                        }
                        else
                        {
                            result.SkippedCount++;
                            _logger.LogWarning($"Estudiante {record.StudentId} ya estaba inscrito en semestre {semesterId}");
                        }
                    }
                    catch (Exception ex)
                    {
                        result.Errors.Add($"Estudiante {record.Name} ({record.StudentId}): {ex.Message}");
                        result.FailureCount++;
                        _logger.LogError($"Error procesando estudiante {record.StudentId}: {ex.Message}");
                    }
                }

                await _context.SaveChangesAsync();
            }
        }
        catch (Exception ex)
        {
            result.HasError = true;
            result.ErrorMessage = ex.Message;
            _logger.LogError($"Error importando CSV: {ex.Message}\n{ex.StackTrace}");
        }

        result.TotalProcessed = result.SuccessCount + result.FailureCount + result.SkippedCount;
        return result;
    }
}

public class StudentCsvDto
{
    public long StudentId { get; set; }
    public string Name { get; set; } = string.Empty;
}

public class StudentCsvMap : ClassMap<StudentCsvDto>
{
    public StudentCsvMap()
    {
        Map(m => m.StudentId)
            .Name("Documento Identidad")
            .Convert(args =>
            {
                var value = args.Row.GetField("Documento Identidad");
                
                // Si comienza con "E-", tomar solo la parte numérica
                if (!string.IsNullOrWhiteSpace(value) && value.StartsWith("E-"))
                {
                    value = value.Substring(2); // Remove "E-"
                }
                
                // Intentar convertir a Int64
                if (long.TryParse(value, out long result))
                {
                    return result;
                }
                
                // Si no se puede convertir, retornar 0 (se omitirá después)
                return 0;
            });
        Map(m => m.Name).Name("Nombre Estudiante");
    }
}

public class ImportResult
{
    public int SuccessCount { get; set; }
    public int FailureCount { get; set; }
    public int SkippedCount { get; set; }
    public int TotalProcessed { get; set; }
    public bool HasError { get; set; }
    public string ErrorMessage { get; set; } = string.Empty;
    public List<string> Errors { get; set; } = new();
}
