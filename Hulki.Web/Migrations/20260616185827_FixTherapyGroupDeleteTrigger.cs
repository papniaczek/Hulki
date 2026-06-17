using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Hulki.Web.Migrations
{
    /// <summary>
    /// Naprawia trg_PreventDeleteNonEmptyGroup, który nie mógł zostać utworzony
    /// jako INSTEAD OF DELETE, bo TherapyGroups jest stroną kilku kaskadowych FK
    /// (PatientGroups, GroupMessages, GroupQuests, GroupResources).
    /// SQL Server nie pozwala na INSTEAD OF DELETE/UPDATE na tabeli z wchodzącą kaskadą.
    ///
    /// Zamiast tego: trigger AFTER DELETE, który nie blokuje usunięcia,
    /// tylko zapisuje log audytowy do TherapyGroupDeletionLogs.
    /// </summary>
    public partial class FixTherapyGroupDeleteTrigger : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            // Na wszelki wypadek – usuń ewentualną wcześniejszą (nieudaną) wersję
            migrationBuilder.Sql("DROP TRIGGER IF EXISTS dbo.trg_PreventDeleteNonEmptyGroup;");

            // ── Tabela audytowa ──────────────────────────────────────────────
            migrationBuilder.CreateTable(
                name: "TherapyGroupDeletionLogs",
                columns: table => new
                {
                    Id               = table.Column<int>(nullable: false)
                                            .Annotation("SqlServer:Identity", "1, 1"),
                    DeletedGroupId   = table.Column<int>(nullable: false),
                    DeletedGroupName = table.Column<string>(maxLength: 100, nullable: false),
                    DeletedAt        = table.Column<System.DateTime>(nullable: false, defaultValueSql: "GETUTCDATE()")
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_TherapyGroupDeletionLogs", x => x.Id);
                });

            // ── Trigger AFTER DELETE (zgodny z kaskadami) ───────────────────
            migrationBuilder.Sql(@"
CREATE OR ALTER TRIGGER dbo.trg_AuditTherapyGroupDelete
ON TherapyGroups
AFTER DELETE
AS
BEGIN
    SET NOCOUNT ON;

    INSERT INTO TherapyGroupDeletionLogs (DeletedGroupId, DeletedGroupName, DeletedAt)
    SELECT d.Id, d.Name, GETDATE()
    FROM deleted d;
END;
");
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.Sql("DROP TRIGGER IF EXISTS dbo.trg_AuditTherapyGroupDelete;");
            migrationBuilder.DropTable(name: "TherapyGroupDeletionLogs");
        }
    }
}