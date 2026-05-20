using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Hulki.Web.Migrations
{
    /// <inheritdoc />
    public partial class AddQuizFields : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "CorrectOption",
                table: "GroupQuests",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "OptionA",
                table: "GroupQuests",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "OptionB",
                table: "GroupQuests",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "OptionC",
                table: "GroupQuests",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "OptionD",
                table: "GroupQuests",
                type: "nvarchar(max)",
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "CorrectOption",
                table: "GroupQuests");

            migrationBuilder.DropColumn(
                name: "OptionA",
                table: "GroupQuests");

            migrationBuilder.DropColumn(
                name: "OptionB",
                table: "GroupQuests");

            migrationBuilder.DropColumn(
                name: "OptionC",
                table: "GroupQuests");

            migrationBuilder.DropColumn(
                name: "OptionD",
                table: "GroupQuests");
        }
    }
}
