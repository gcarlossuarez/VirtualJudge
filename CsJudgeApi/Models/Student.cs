using System;
using System.Collections.Generic;

namespace CsJudgeApi.Models;

public class Student
{
    public long StudentId { get; set; }
    public string Name { get; set; } = string.Empty;

    // Relación con Contest
    public ICollection<ContestStudent> Contests { get; set; } = new List<ContestStudent>();
}

