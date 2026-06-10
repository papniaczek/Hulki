using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Hulki.Web.Migrations
{
    /// <inheritdoc />
    public partial class ExpandBadgesSystem : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AlterColumn<string>(
                name: "AppUserId",
                table: "UserBadges",
                type: "nvarchar(450)",
                nullable: false,
                oldClrType: typeof(string),
                oldType: "nvarchar(max)");

            migrationBuilder.AddColumn<DateTime>(
                name: "EarnedAt",
                table: "UserBadges",
                type: "datetime2",
                nullable: false,
                defaultValue: new DateTime(1, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified));

            migrationBuilder.AddColumn<string>(
                name: "ConditionType",
                table: "AchievementBadges",
                type: "nvarchar(max)",
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<int>(
                name: "ConditionValue",
                table: "AchievementBadges",
                type: "int",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<string>(
                name: "Description",
                table: "AchievementBadges",
                type: "nvarchar(max)",
                nullable: false,
                defaultValue: "");

            migrationBuilder.CreateIndex(
                name: "IX_UserBadges_AppUserId",
                table: "UserBadges",
                column: "AppUserId");

            migrationBuilder.CreateIndex(
                name: "IX_UserBadges_BadgeId",
                table: "UserBadges",
                column: "BadgeId");

            migrationBuilder.AddForeignKey(
                name: "FK_UserBadges_AchievementBadges_BadgeId",
                table: "UserBadges",
                column: "BadgeId",
                principalTable: "AchievementBadges",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_UserBadges_AspNetUsers_AppUserId",
                table: "UserBadges",
                column: "AppUserId",
                principalTable: "AspNetUsers",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_UserBadges_AchievementBadges_BadgeId",
                table: "UserBadges");

            migrationBuilder.DropForeignKey(
                name: "FK_UserBadges_AspNetUsers_AppUserId",
                table: "UserBadges");

            migrationBuilder.DropIndex(
                name: "IX_UserBadges_AppUserId",
                table: "UserBadges");

            migrationBuilder.DropIndex(
                name: "IX_UserBadges_BadgeId",
                table: "UserBadges");

            migrationBuilder.DropColumn(
                name: "EarnedAt",
                table: "UserBadges");

            migrationBuilder.DropColumn(
                name: "ConditionType",
                table: "AchievementBadges");

            migrationBuilder.DropColumn(
                name: "ConditionValue",
                table: "AchievementBadges");

            migrationBuilder.DropColumn(
                name: "Description",
                table: "AchievementBadges");

            migrationBuilder.AlterColumn<string>(
                name: "AppUserId",
                table: "UserBadges",
                type: "nvarchar(max)",
                nullable: false,
                oldClrType: typeof(string),
                oldType: "nvarchar(450)");
        }
    }
}
