using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace CsJudgeApi.Migrations
{
    /// <inheritdoc />
    /// <summary>
    /// Migración para agregar la relación muchos a muchos entre Contest y Question mediante ContestQuestion.
    /// </summary>
    public partial class AddContestQuestionRelationship : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Questions_Contests_ContestId",
                table: "Questions");

            migrationBuilder.DropIndex(
                name: "IX_Questions_ContestId",
                table: "Questions");

            migrationBuilder.DropColumn(
                name: "ContestId",
                table: "Questions");

            migrationBuilder.CreateTable(
                name: "ContestQuestions",
                columns: table => new
                {
                    ContestId = table.Column<int>(type: "INTEGER", nullable: false),
                    QuestionId = table.Column<int>(type: "INTEGER", nullable: false),
                    Order = table.Column<int>(type: "INTEGER", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ContestQuestions", x => new { x.ContestId, x.QuestionId });
                    table.ForeignKey(
                        name: "FK_ContestQuestions_Contests_ContestId",
                        column: x => x.ContestId,
                        principalTable: "Contests",
                        principalColumn: "ContestId",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_ContestQuestions_Questions_QuestionId",
                        column: x => x.QuestionId,
                        principalTable: "Questions",
                        principalColumn: "QuestionId",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_ContestQuestion_ContestOrder",
                table: "ContestQuestions",
                columns: new[] { "ContestId", "Order" });

            migrationBuilder.CreateIndex(
                name: "IX_ContestQuestions_QuestionId",
                table: "ContestQuestions",
                column: "QuestionId");
        }

        /// <inheritdoc />
        /// <summary>
        /// Revertir los cambios realizados en la migración Up.
        /// </summary>
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "ContestQuestions");

            migrationBuilder.AddColumn<int>(
                name: "ContestId",
                table: "Questions",
                type: "INTEGER",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.CreateIndex(
                name: "IX_Questions_ContestId",
                table: "Questions",
                column: "ContestId");

            migrationBuilder.AddForeignKey(
                name: "FK_Questions_Contests_ContestId",
                table: "Questions",
                column: "ContestId",
                principalTable: "Contests",
                principalColumn: "ContestId",
                onDelete: ReferentialAction.Cascade);
        }
    }
}
