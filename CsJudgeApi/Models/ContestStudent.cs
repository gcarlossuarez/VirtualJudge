using System;
using System.Collections.Generic;

namespace CsJudgeApi.Models;

public class ContestStudent
{
    public int ContestId { get; set; }
    public Contest Contest { get; set; } = null!;

    public long StudentId { get; set; }
    public Student Student { get; set; } = null!;

    public DateTime DateParticipation { get; set; }
    public string IP { get; set; } = string.Empty;
}


