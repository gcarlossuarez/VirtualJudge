namespace CsJudgeApi.Models;

public class Submission
{
    public int SubmissionId { get; set; }   // PK autoincremental

    // Identificación
    public long StudentId { get; set; } = 0;
    public string ProblemId { get; set; } = string.Empty;

    // Código fuente enviado
    public string SourceCode { get; set; } = string.Empty;

    // Comparación de salida
    public string OutputExpected { get; set; } = string.Empty;
    public string OutputActual { get; set; } = string.Empty;
    public bool IsCorrect { get; set; }

    // Logs y diagnósticos
    public string BuildLog { get; set; } = string.Empty;
    public string RunLog { get; set; } = string.Empty;
    public string? Time { get; set; }
    public string? MemKb { get; set; }
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public string IP { get; set; } = string.Empty;
}

