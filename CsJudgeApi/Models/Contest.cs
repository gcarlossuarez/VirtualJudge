using System;
using System.Collections.Generic;

namespace CsJudgeApi.Models;

public class Contest
{
    public int ContestId { get; set; }
    public DateTime Date { get; set; }

    // Relaciona los contests con los lenguajes que soporta
    public List<ContestLanguage> Languages { get; set; } = new();

    // Relación con estudiantes
    public ICollection<ContestStudent> Students { get; set; } = new List<ContestStudent>();

    // Relación M:N con preguntas a través de ContestQuestion
    // Permite reutilizar preguntas en múltiples contests y semestres
    public ICollection<ContestQuestion> Questions { get; set; } = new List<ContestQuestion>();

    public int SemesterId { get; set; }        // FK a Semester
    public int Group { get; set; }             // 1 o 2

    // Navegación
    public Semester? Semester { get; set; }
}


