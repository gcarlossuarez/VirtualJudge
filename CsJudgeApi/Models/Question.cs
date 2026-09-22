using System;
using System.Collections.Generic;

namespace CsJudgeApi.Models;

public class Question
{
    public int QuestionId { get; set; }
    public string Review { get; set; } = string.Empty;
    public string Path { get; set; } = string.Empty;

    public string FullPathValidatorSourceCode { get; set; } = string.Empty;

    public int? TimeLimitSeconds { get; set; }
    
    /// <summary>
    /// Relación M:N con Contests a través de ContestQuestion.
    /// Una pregunta puede ser reutilizada en múltiples contests y semestres.
    /// </summary>
    public ICollection<ContestQuestion> Contests { get; set; } = new List<ContestQuestion>();
}

