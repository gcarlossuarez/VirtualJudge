using CsJudgeApi.Models.Enums;

namespace CsJudgeApi.Models;

public class ActivityLog
{
    public int Id { get; set; }
    
    public DateTime Timestamp { get; set; } = DateTime.Now;
    
    public EActivityAction Action { get; set; }
    
    // Referencias opcionales (nullables para acciones sin contexto específico)
    public long? StudentId { get; set; }
    public int? ContestId { get; set; }
    public int? QuestionId { get; set; }
    
    // Metadata adicional en formato JSON para flexibilidad
    public string? Metadata { get; set; }
    
    // Información de sesión
    public string? IpAddress { get; set; }
    public string? UserAgent { get; set; }
    
    // Navegación
    public Student? Student { get; set; }
    public Contest? Contest { get; set; }
    public Question? Question { get; set; }
}
