using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Hulki.Web.Migrations
{
    /// <inheritdoc />
    public partial class AddGroupQuests : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "GroupQuests",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    TherapyGroupId = table.Column<int>(type: "int", nullable: false),
                    Title = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    Description = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    QuestType = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    RewardPoints = table.Column<int>(type: "int", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_GroupQuests", x => x.Id);
                    table.ForeignKey(
                        name: "FK_GroupQuests_TherapyGroups_TherapyGroupId",
                        column: x => x.TherapyGroupId,
                        principalTable: "TherapyGroups",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "QuestSubmissions",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    GroupQuestId = table.Column<int>(type: "int", nullable: false),
                    AppUserId = table.Column<string>(type: "nvarchar(450)", nullable: false),
                    AnswerText = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    SubmittedAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                    IsEvaluated = table.Column<bool>(type: "bit", nullable: false),
                    IsAccepted = table.Column<bool>(type: "bit", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_QuestSubmissions", x => x.Id);
                    table.ForeignKey(
                        name: "FK_QuestSubmissions_AspNetUsers_AppUserId",
                        column: x => x.AppUserId,
                        principalTable: "AspNetUsers",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_QuestSubmissions_GroupQuests_GroupQuestId",
                        column: x => x.GroupQuestId,
                        principalTable: "GroupQuests",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_GroupQuests_TherapyGroupId",
                table: "GroupQuests",
                column: "TherapyGroupId");

            migrationBuilder.CreateIndex(
                name: "IX_QuestSubmissions_AppUserId",
                table: "QuestSubmissions",
                column: "AppUserId");

            migrationBuilder.CreateIndex(
                name: "IX_QuestSubmissions_GroupQuestId",
                table: "QuestSubmissions",
                column: "GroupQuestId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "QuestSubmissions");

            migrationBuilder.DropTable(
                name: "GroupQuests");
        }
    }
}
