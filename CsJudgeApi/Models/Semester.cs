using System;
using System.Collections.Generic;

namespace CsJudgeApi.Models;

public class Semester
{
    public int SemesterId { get; set; }
    public int SemesterCode { get; set; }          // 20262, 20252
    public string Name { get; set; } = string.Empty;  // "2-2026", "2-2025"
    public DateTime StartDate { get; set; }
    public DateTime EndDate { get; set; }
    public bool IsActive { get; set; }

    // Relaciones
    public ICollection<StudentSemester> Students { get; set; } = new List<StudentSemester>();
    public ICollection<Contest> Contests { get; set; } = new List<Contest>();
}
