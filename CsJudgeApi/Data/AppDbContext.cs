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
