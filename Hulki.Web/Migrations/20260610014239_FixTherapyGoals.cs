using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Hulki.Web.Migrations
{
    /// <inheritdoc />
    public partial class FixTherapyGoals : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AlterColumn<string>(
                name: "AppUserId",
                table: "TherapyGoals",
                type: "nvarchar(450)",
                nullable: false,
                oldClrType: typeof(string),
                oldType: "nvarchar(max)");

            migrationBuilder.AddColumn<bool>(
                name: "IsCompleted",
                table: "GoalMilestones",
                type: "bit",
                nullable: false,
                defaultValue: false);

            migrationBuilder.CreateIndex(
                name: "IX_TherapyGoals_AppUserId",
                table: "TherapyGoals",
                column: "AppUserId");

            migrationBuilder.CreateIndex(
                name: "IX_GoalMilestones_GoalId",
                table: "GoalMilestones",
                column: "GoalId");

            migrationBuilder.AddForeignKey(
                name: "FK_GoalMilestones_TherapyGoals_GoalId",
                table: "GoalMilestones",
                column: "GoalId",
                principalTable: "TherapyGoals",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_TherapyGoals_AspNetUsers_AppUserId",
                table: "TherapyGoals",
                column: "AppUserId",
                principalTable: "AspNetUsers",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_GoalMilestones_TherapyGoals_GoalId",
                table: "GoalMilestones");

            migrationBuilder.DropForeignKey(
                name: "FK_TherapyGoals_AspNetUsers_AppUserId",
                table: "TherapyGoals");

            migrationBuilder.DropIndex(
                name: "IX_TherapyGoals_AppUserId",
                table: "TherapyGoals");

            migrationBuilder.DropIndex(
                name: "IX_GoalMilestones_GoalId",
                table: "GoalMilestones");

            migrationBuilder.DropColumn(
                name: "IsCompleted",
                table: "GoalMilestones");

            migrationBuilder.AlterColumn<string>(
                name: "AppUserId",
                table: "TherapyGoals",
                type: "nvarchar(max)",
                nullable: false,
                oldClrType: typeof(string),
                oldType: "nvarchar(450)");
        }
    }
}
