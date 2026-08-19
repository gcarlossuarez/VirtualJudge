using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

#pragma warning disable CA1814 // Prefer jagged arrays over multidimensional

namespace CsJudgeApi.Migrations
{
    /// <inheritdoc />
    public partial class AddSemesterAndStudentSemesterSupport : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            // 1️⃣ Crear tabla Semesters PRIMERO (sin FK)
            migrationBuilder.CreateTable(
                name: "Semesters",
                columns: table => new
                {
                    SemesterId = table.Column<int>(type: "INTEGER", nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    SemesterCode = table.Column<int>(type: "INTEGER", nullable: false),
                    Name = table.Column<string>(type: "TEXT", nullable: false),
                    StartDate = table.Column<DateTime>(type: "TEXT", nullable: false),
                    EndDate = table.Column<DateTime>(type: "TEXT", nullable: false),
                    IsActive = table.Column<bool>(type: "INTEGER", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Semesters", x => x.SemesterId);
                });

            // 2️⃣ Insertar datos de Semesters ANTES de crear FK
            migrationBuilder.InsertData(
                table: "Semesters",
                columns: new[] { "SemesterId", "EndDate", "IsActive", "Name", "SemesterCode", "StartDate" },
                values: new object[,]
                {
                    { 1, new DateTime(2025, 12, 20, 0, 0, 0, 0, DateTimeKind.Unspecified), false, "2-2025", 20252, new DateTime(2025, 8, 4, 0, 0, 0, 0, DateTimeKind.Unspecified) },
                    { 2, new DateTime(2026, 12, 19, 0, 0, 0, 0, DateTimeKind.Unspecified), true, "2-2026", 20262, new DateTime(2026, 8, 3, 0, 0, 0, 0, DateTimeKind.Unspecified) }
                });

            // 3️⃣ Crear tabla StudentSemesters
            migrationBuilder.CreateTable(
                name: "StudentSemesters",
                columns: table => new
                {
                    StudentId = table.Column<long>(type: "INTEGER", nullable: false),
                    SemesterId = table.Column<int>(type: "INTEGER", nullable: false),
                    Group = table.Column<int>(type: "INTEGER", nullable: false),
                    ParallelName = table.Column<string>(type: "TEXT", nullable: false),
                    EnrollmentDate = table.Column<DateTime>(type: "TEXT", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_StudentSemesters", x => new { x.StudentId, x.SemesterId });
                    table.ForeignKey(
                        name: "FK_StudentSemesters_Semesters_SemesterId",
                        column: x => x.SemesterId,
                        principalTable: "Semesters",
                        principalColumn: "SemesterId",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_StudentSemesters_Students_StudentId",
                        column: x => x.StudentId,
                        principalTable: "Students",
                        principalColumn: "StudentId",
                        onDelete: ReferentialAction.Cascade);
                });

            // 5️⃣ Crear índices
            migrationBuilder.CreateIndex(
                name: "IX_Contest_SemesterGroup",
                table: "Contests",
                columns: new[] { "SemesterId", "Group" });

            migrationBuilder.CreateIndex(
                name: "IX_StudentSemester_SemesterGroup",
                table: "StudentSemesters",
                columns: new[] { "SemesterId", "Group" });

            // 6️⃣ Migrar estudiantes existentes (2025-2, Paralelo 1, SemesterId=1)
            migrationBuilder.Sql(@"
                INSERT INTO StudentSemesters (StudentId, SemesterId, ""Group"", ParallelName, EnrollmentDate)
                SELECT DISTINCT StudentId, 1, 1, 'Paralelo 1', datetime('now')
                FROM Students
                WHERE StudentId NOT IN (SELECT DISTINCT StudentId FROM StudentSemesters);
            ");

            // 7️⃣ Agregar columnas a Contests DESPUÉS de que Semesters existe
            migrationBuilder.AddColumn<int>(
                name: "Group",
                table: "Contests",
                type: "INTEGER",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<int>(
                name: "SemesterId",
                table: "Contests",
                type: "INTEGER",
                nullable: false,
                defaultValue: 1);

            // 8️⃣ Agregar FK a Contests AL FINAL
            migrationBuilder.AddForeignKey(
                name: "FK_Contests_Semesters_SemesterId",
                table: "Contests",
                column: "SemesterId",
                principalTable: "Semesters",
                principalColumn: "SemesterId",
                onDelete: ReferentialAction.Cascade);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Contests_Semesters_SemesterId",
                table: "Contests");

            migrationBuilder.DropIndex(
                name: "IX_Contest_SemesterGroup",
                table: "Contests");

            migrationBuilder.DropColumn(
                name: "Group",
                table: "Contests");

            migrationBuilder.DropColumn(
                name: "SemesterId",
                table: "Contests");

            migrationBuilder.DropTable(name: "StudentSemesters");
            migrationBuilder.DropTable(name: "Semesters");
        }
    }
}
