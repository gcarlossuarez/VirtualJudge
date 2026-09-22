using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace CsJudgeApi.Migrations
{
    /// <inheritdoc />
    public partial class AddAllowCopyPasteToContestQuestion : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<bool>(
                name: "AllowCopyPaste",
                table: "ContestQuestions",
                type: "INTEGER",
                nullable: false,
                defaultValue: true);  // ✨ Permitir copy/paste por defecto
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "AllowCopyPaste",
                table: "ContestQuestions");
        }
    }
}
