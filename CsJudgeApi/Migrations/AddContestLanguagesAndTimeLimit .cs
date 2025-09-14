using Microsoft.EntityFrameworkCore.Migrations;
public partial class AddContestLanguagesAndTimeLimit : Migration
{
    protected override void Up(MigrationBuilder migrationBuilder)
    {
        // Crear tabla ContestLanguages
        migrationBuilder.CreateTable(
            name: "ContestLanguages",
            columns: table => new
            {
                ContestId = table.Column<int>(nullable: false),
                Language = table.Column<string>(nullable: false)
            },
            constraints: table =>
            {
                table.PrimaryKey("PK_ContestLanguages", x => new { x.ContestId, x.Language });
                table.ForeignKey(
                    name: "FK_ContestLanguages_Contests_ContestId",
                    column: x => x.ContestId,
                    principalTable: "Contests",
                    principalColumn: "Id",
                    onDelete: ReferentialAction.Cascade);
            });

        // Agregar campo a Questions
        migrationBuilder.AddColumn<int>(
            name: "TimeLimitSeconds",
            table: "Questions",
            nullable: true);

        // Seed inicial: todos los contests existentes quedan con CSharp habilitado
        migrationBuilder.Sql("INSERT INTO ContestLanguages (ContestId, Language) SELECT Id, 'CSharp' FROM Contests");
    }

    protected override void Down(MigrationBuilder migrationBuilder)
    {
        // Quitar campo de Questions
        migrationBuilder.DropColumn(
            name: "TimeLimitSeconds",
            table: "Questions");

        // Borrar tabla ContestLanguages
        migrationBuilder.DropTable(
            name: "ContestLanguages");
    }
}
