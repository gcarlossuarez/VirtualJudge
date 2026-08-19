using System;

namespace CsJudgeApi.Models;

public class StudentSemester
{
    public long StudentId { get; set; }
    public int SemesterId { get; set; }
    public int Group { get; set; }                 // 1 o 2
    public string ParallelName { get; set; } = string.Empty;
    public DateTime EnrollmentDate { get; set; } = DateTime.Now;

    // Navegación
    public Student? Student { get; set; }
    public Semester? Semester { get; set; }
}