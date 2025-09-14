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
    
    public int ContestId { get; set; }
    public Contest Contest { get; set; } = null!;
}

