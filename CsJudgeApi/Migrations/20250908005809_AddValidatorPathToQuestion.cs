using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace CsJudgeApi.Migrations
{
    /// <inheritdoc />
    public partial class AddValidatorPathToQuestion : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "FullPathValidatorSourceCode",
                table: "Questions",
                type: "TEXT",
                nullable: false,
                defaultValue: "");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "FullPathValidatorSourceCode",
                table: "Questions");
        }
    }
}
