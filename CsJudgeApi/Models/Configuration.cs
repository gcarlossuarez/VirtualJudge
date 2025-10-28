namespace CsJudgeApi.Models
{
    public class Configuration
    {
        public string Key { get; set; } = null!;  // Clave primaria
        public string? Value { get; set; }
        public string? Description { get; set; }
    }
}

