using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Hulki.Web.Migrations
{
    /// <inheritdoc />
    public partial class SyncMoodLogModelSnapshot : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            // AlterColumn (nvarchar(max) -> nvarchar(450)) i AddForeignKey
            // zostały już wykonane ręcznym SQL-em w migracji
            // AddPerformanceIndexes — usunięte stąd, żeby nie próbować
            // tworzyć ich drugi raz (błąd 2714: "already an object named...").
            //
            // Zostaje tylko CreateIndex na samym AppUserId — to jest
            // standardowy indeks pod klucz obcy, który EF chce mieć
            // niezależnie od naszego indeksu złożonego
            // IX_MoodLogs_AppUserId_Date (ten drugi obsługuje filtr+sort,
            // ten tu obsługuje samo wskazanie FK / inne zapytania równościowe).
            migrationBuilder.CreateIndex(
                name: "IX_MoodLogs_AppUserId",
                table: "MoodLogs",
                column: "AppUserId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_MoodLogs_AppUserId",
                table: "MoodLogs");
        }
    }
}