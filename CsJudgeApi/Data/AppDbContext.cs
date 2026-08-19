using Microsoft.EntityFrameworkCore;
using CsJudgeApi.Models;
namespace CsJudgeApi.Data;

public class AppDbContext : DbContext
{
    public DbSet<Student> Students { get; set; } = null!;
    public DbSet<Contest> Contests { get; set; } = null!;
    public DbSet<ContestStudent> ContestStudents { get; set; } = null!;
    public DbSet<Question> Questions { get; set; } = null!;
    public DbSet<Submission> Submissions => Set<Submission>();
    // 🔹 Otras tablas ya existentes
    public DbSet<Configuration> Configurations => Set<Configuration>();
    public DbSet<ActivityLog> ActivityLogs => Set<ActivityLog>();

    public DbSet<Semester> Semesters { get; set; } = null!;
    public DbSet<StudentSemester> StudentSemesters { get; set; } = null!;


    public AppDbContext(DbContextOptions<AppDbContext> options)
        : base(options) { }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        // PK compuesta en ContestStudent
        modelBuilder.Entity<ContestStudent>()
            .HasKey(ce => new { ce.ContestId, ce.StudentId });

        // Relaciones ContestStudent <-> Student
        modelBuilder.Entity<ContestStudent>()
            .HasOne(ce => ce.Student)
            .WithMany(e => e.Contests)
            .HasForeignKey(ce => ce.StudentId);

        // Relaciones ContestStudent <-> Contest
        modelBuilder.Entity<ContestStudent>()
            .HasOne(ce => ce.Contest)
            .WithMany(c => c.Students)
            .HasForeignKey(ce => ce.ContestId);

        // Relación Contest <-> Pregunta
        modelBuilder.Entity<Question>()
            .HasOne(p => p.Contest)
            .WithMany(c => c.Questions)
            .HasForeignKey(p => p.ContestId);

        // PK compuesta en ContestLanguage
        modelBuilder.Entity<ContestLanguage>()
        .HasKey(cl => new { cl.ContestId, cl.Language });

        // Relación Contest <-> ContestLanguage
        modelBuilder.Entity<ContestLanguage>()
            .HasOne(cl => cl.Contest)
            .WithMany(c => c.Languages)
            .HasForeignKey(cl => cl.ContestId);

        // Configuración explícita de la clave primaria en la tabla Configuration
        modelBuilder.Entity<Configuration>(entity =>
        {
            entity.HasKey(e => e.Key);
            entity.Property(e => e.Key)
                    .HasMaxLength(50)
                    .IsRequired();

            entity.Property(e => e.Value)
                    .HasMaxLength(250);

            entity.Property(e => e.Description)
                    .HasMaxLength(500);
        });

        // ========== NUEVA CONFIGURACIÓN M:N ==========

        // PK compuesta en StudentSemester
        modelBuilder.Entity<StudentSemester>()
            .HasKey(ss => new { ss.StudentId, ss.SemesterId });

        // Relaciones StudentSemester -> Student
        modelBuilder.Entity<StudentSemester>()
            .HasOne(ss => ss.Student)
            .WithMany(s => s.Semesters)
            .HasForeignKey(ss => ss.StudentId)
            .OnDelete(DeleteBehavior.Cascade);

        // Relaciones StudentSemester -> Semester
        modelBuilder.Entity<StudentSemester>()
            .HasOne(ss => ss.Semester)
            .WithMany(s => s.Students)
            .HasForeignKey(ss => ss.SemesterId)
            .OnDelete(DeleteBehavior.Cascade);

        // Índices de búsqueda
        modelBuilder.Entity<StudentSemester>()
            .HasIndex(ss => new { ss.SemesterId, ss.Group })
            .HasDatabaseName("IX_StudentSemester_SemesterGroup");

        // Relación Contest -> Semester
        modelBuilder.Entity<Contest>()
            .HasOne(c => c.Semester)
            .WithMany(s => s.Contests)
            .HasForeignKey(c => c.SemesterId)
            .OnDelete(DeleteBehavior.Cascade);

        // Índice en Contest
        modelBuilder.Entity<Contest>()
            .HasIndex(c => new { c.SemesterId, c.Group })
            .HasDatabaseName("IX_Contest_SemesterGroup");

        // ========== SEED DATA: SEMESTERS ==========
        modelBuilder.Entity<Semester>().HasData(
            new Semester 
            { 
                SemesterId = 1,
                SemesterCode = 20252,
                Name = "2-2025",
                StartDate = new DateTime(2025, 8, 4),
                EndDate = new DateTime(2025, 12, 20),
                IsActive = false
            },
            new Semester 
            { 
                SemesterId = 2,
                SemesterCode = 20262,
                Name = "2-2026",
                StartDate = new DateTime(2026, 8, 3),
                EndDate = new DateTime(2026, 12, 19),
                IsActive = true
            }
        );
    }
}

/*
using Microsoft.EntityFrameworkCore;
using CsJudgeApi.Models;

namespace CsJudgeApi.Data;

public class AppDbContext : DbContext
{
    public AppDbContext(DbContextOptions<AppDbContext> options) : base(options) { }

    public DbSet<Submission> Submissions => Set<Submission>();
}
*/
