using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Hulki.Web.Migrations
{
    /// <summary>
    /// Indeksy optymalizacyjne — dodane po analizie najczęstszych zapytań
    /// w aplikacji (patrz: dokumentacja, rozdział "Optymalizacja bazy danych").
    ///
    /// 1. IX_MoodLogs_AppUserId_Date
    ///    MoodLog.AppUserId nie miał ŻADNEGO indeksu (kolumna nie jest
    ///    deklarowana jako [ForeignKey] w modelu C#, więc EF nie wygenerował
    ///    indeksu automatycznie). Zapytanie "wpisy nastroju pacjenta,
    ///    sortowane chronologicznie" wymuszało Clustered Index Scan na całej
    ///    tabeli. Indeks złożony (AppUserId, Date) pokrywa filtr + sortowanie.
    ///
    /// 2. IX_ForumPosts_ForumTopicId_CreatedAt
    ///    Istniejący IX_ForumPosts_ForumTopicId obsługuje tylko filtr,
    ///    a stronicowanie (Topic.cshtml) dodatkowo sortuje po CreatedAt.
    ///    Indeks złożony eliminuje sortowanie w pamięci (Sort operator)
    ///    przy każdej stronie wyników.
    ///
    /// 3. IX_Consultations_PatientId_StartTime / IX_Consultations_TherapistId_StartTime
    ///    Lista konsultacji pacjenta/terapeuty jest zawsze sortowana
    ///    chronologicznie (ConsultationService) — indeksy złożone
    ///    pokrywają filtr po stronie + sortowanie jednym przejściem.
    /// </summary>
    public partial class AddPerformanceIndexes : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            // ── Naprawa typu kolumny przed założeniem na niej indeksu ───────
            // MoodLogs.AppUserId został wygenerowany jako nvarchar(max),
            // bo w modelu C# (MoodLog.cs) nie miał atrybutu [ForeignKey]
            // ani [MaxLength] — SQL Server nie pozwala użyć nvarchar(max)
            // jako kolumny kluczowej indeksu (błąd 1919). Zmieniamy na
            // nvarchar(450), czyli ten sam typ co AspNetUsers.Id, i przy
            // tym dodajemy prawdziwy klucz obcy, którego wcześniej brakowało
            // (oryginalny model nie miał relacji nawigacyjnej do AppUser).
            migrationBuilder.Sql(@"
ALTER TABLE dbo.MoodLogs
ALTER COLUMN AppUserId NVARCHAR(450) NOT NULL;
");

            // Zabezpieczenie: jeśli istnieją "osierocone" wpisy MoodLogs
            // (AppUserId, który nie odpowiada żadnemu AspNetUsers.Id — np.
            // po ręcznym usunięciu użytkownika), ADD CONSTRAINT poniżej
            // by się wywalił z kryptycznym błędem. Usuwamy je wcześniej.
            migrationBuilder.Sql(@"
DELETE ml FROM dbo.MoodLogs ml
WHERE NOT EXISTS (
    SELECT 1 FROM dbo.AspNetUsers u WHERE u.Id = ml.AppUserId
);
");

            migrationBuilder.Sql(@"
IF NOT EXISTS (
    SELECT 1 FROM sys.foreign_keys WHERE name = 'FK_MoodLogs_AspNetUsers_AppUserId'
)
ALTER TABLE dbo.MoodLogs
ADD CONSTRAINT FK_MoodLogs_AspNetUsers_AppUserId
    FOREIGN KEY (AppUserId) REFERENCES dbo.AspNetUsers(Id)
    ON DELETE CASCADE;
");

            migrationBuilder.Sql(@"
IF NOT EXISTS (SELECT 1 FROM sys.indexes WHERE name = 'IX_MoodLogs_AppUserId_Date')
    CREATE NONCLUSTERED INDEX IX_MoodLogs_AppUserId_Date
    ON dbo.MoodLogs (AppUserId, Date DESC);
");

            migrationBuilder.Sql(@"
IF NOT EXISTS (SELECT 1 FROM sys.indexes WHERE name = 'IX_ForumPosts_ForumTopicId_CreatedAt')
    CREATE NONCLUSTERED INDEX IX_ForumPosts_ForumTopicId_CreatedAt
    ON dbo.ForumPosts (ForumTopicId, CreatedAt);
");

            migrationBuilder.Sql(@"
IF NOT EXISTS (SELECT 1 FROM sys.indexes WHERE name = 'IX_Consultations_PatientId_StartTime')
    CREATE NONCLUSTERED INDEX IX_Consultations_PatientId_StartTime
    ON dbo.Consultations (PatientId, StartTime DESC);
");

            migrationBuilder.Sql(@"
IF NOT EXISTS (SELECT 1 FROM sys.indexes WHERE name = 'IX_Consultations_TherapistId_StartTime')
    CREATE NONCLUSTERED INDEX IX_Consultations_TherapistId_StartTime
    ON dbo.Consultations (TherapistId, StartTime DESC);
");

            migrationBuilder.Sql(@"
IF NOT EXISTS (SELECT 1 FROM sys.indexes WHERE name = 'IX_DailyReports_AppUserId_CreatedAt')
    CREATE NONCLUSTERED INDEX IX_DailyReports_AppUserId_CreatedAt
    ON dbo.DailyReports (AppUserId, CreatedAt DESC);
");
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.Sql("DROP INDEX IF EXISTS IX_MoodLogs_AppUserId_Date ON dbo.MoodLogs;");
            migrationBuilder.Sql("DROP INDEX IF EXISTS IX_ForumPosts_ForumTopicId_CreatedAt ON dbo.ForumPosts;");
            migrationBuilder.Sql("DROP INDEX IF EXISTS IX_Consultations_PatientId_StartTime ON dbo.Consultations;");
            migrationBuilder.Sql("DROP INDEX IF EXISTS IX_Consultations_TherapistId_StartTime ON dbo.Consultations;");
            migrationBuilder.Sql("DROP INDEX IF EXISTS IX_DailyReports_AppUserId_CreatedAt ON dbo.DailyReports;");

            // Usuń FK przed ewentualnym przywróceniem starego typu kolumny —
            // SQL Server nie pozwoli zmienić typu kolumny, na której
            // istnieje aktywny klucz obcy.
            migrationBuilder.Sql(@"
IF EXISTS (SELECT 1 FROM sys.foreign_keys WHERE name = 'FK_MoodLogs_AspNetUsers_AppUserId')
    ALTER TABLE dbo.MoodLogs DROP CONSTRAINT FK_MoodLogs_AspNetUsers_AppUserId;
");
            // Typ kolumny celowo NIE jest przywracany do nvarchar(max) —
            // nvarchar(450) jest poprawnym, węższym typem i jego pozostanie
            // nie psuje żadnych danych ani zapytań.
        }
    }
}