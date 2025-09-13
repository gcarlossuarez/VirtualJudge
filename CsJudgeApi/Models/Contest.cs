using System;
using System.Collections.Generic;

namespace CsJudgeApi.Models;

public class Contest
{
    public int ContestId { get; set; }
    public DateTime Date { get; set; }

    // Relación con estudiantes
    public ICollection<ContestStudent> Students { get; set; } = new List<ContestStudent>();

    // Relación con preguntas
    public ICollection<Question> Questions { get; set; } = new List<Question>();
}


