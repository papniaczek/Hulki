using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Hulki.Web.Migrations
{
    /// <inheritdoc />
    public partial class AddCustomUsersAndSqlObjects : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            // ════════════════════════════════════════════════════════════════
            //  1. WŁASNA TABELA UŻYTKOWNIKÓW
            // ════════════════════════════════════════════════════════════════
            migrationBuilder.CreateTable(
                name: "CustomUsers",
                columns: table => new
                {
                    Id           = table.Column<int>(nullable: false)
                                        .Annotation("SqlServer:Identity", "1, 1"),
                    FirstName    = table.Column<string>(maxLength: 100, nullable: false),
                    LastName     = table.Column<string>(maxLength: 100, nullable: false),
                    Email        = table.Column<string>(maxLength: 200, nullable: false),
                    PasswordHash = table.Column<string>(nullable: false),
                    IsTherapist  = table.Column<bool>(nullable: false, defaultValue: false),
                    CreatedAt    = table.Column<DateTime>(nullable: false, defaultValueSql: "GETUTCDATE()"),
                    AspNetUserId = table.Column<string>(maxLength: 450, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_CustomUsers", x => x.Id);
                });

            migrationBuilder.CreateIndex(
                name: "IX_CustomUsers_Email",
                table: "CustomUsers",
                column: "Email",
                unique: true);

            // ════════════════════════════════════════════════════════════════
            //  2. FUNKCJE SQL
            // ════════════════════════════════════════════════════════════════

            // Funkcja: zwraca pełne imię i nazwisko użytkownika
            migrationBuilder.Sql(@"
CREATE OR ALTER FUNCTION dbo.fn_GetFullName(@userId NVARCHAR(450))
RETURNS NVARCHAR(201)
AS
BEGIN
    DECLARE @name NVARCHAR(201);
    SELECT @name = FirstName + ' ' + LastName
    FROM   AspNetUsers
    WHERE  Id = @userId;
    RETURN ISNULL(@name, 'Nieznany użytkownik');
END;
");

            // Funkcja: zlicza konsultacje użytkownika w zadanym statusie
            migrationBuilder.Sql(@"
CREATE OR ALTER FUNCTION dbo.fn_CountUserConsultations(
    @userId   NVARCHAR(450),
    @statusId INT
)
RETURNS INT
AS
BEGIN
    DECLARE @cnt INT;
    SELECT @cnt = COUNT(*)
    FROM   Consultations
    WHERE  (PatientId = @userId OR TherapistId = @userId)
      AND  StatusId   = @statusId;
    RETURN ISNULL(@cnt, 0);
END;
");

            // Funkcja: oblicza średnie saldo portfela (przydatna w raportach)
            migrationBuilder.Sql(@"
CREATE OR ALTER FUNCTION dbo.fn_AverageWalletBalance()
RETURNS DECIMAL(18,2)
AS
BEGIN
    DECLARE @avg DECIMAL(18,2);
    SELECT @avg = AVG(CAST(Balance AS DECIMAL(18,2))) FROM Wallets;
    RETURN ISNULL(@avg, 0);
END;
");

            // ════════════════════════════════════════════════════════════════
            //  3. PROCEDURY SKŁADOWANE
            // ════════════════════════════════════════════════════════════════

            // Procedura: zwraca statystyki użytkownika
            migrationBuilder.Sql(@"
CREATE OR ALTER PROCEDURE dbo.sp_GetUserStats
    @userId NVARCHAR(450)
AS
BEGIN
    SET NOCOUNT ON;

    SELECT
        dbo.fn_GetFullName(@userId)                                        AS FullName,
        dbo.fn_CountUserConsultations(@userId, 2)                          AS CompletedConsultations,
        (SELECT COUNT(*) FROM MoodLogs     WHERE AppUserId = @userId)      AS MoodLogCount,
        (SELECT COUNT(*) FROM DailyReports WHERE AppUserId = @userId)      AS DailyReportCount,
        (SELECT COUNT(*) FROM TherapyGoals WHERE AppUserId = @userId
                                            AND IsCompleted = 1)           AS CompletedGoals,
        (SELECT ISNULL(Balance, 0) FROM Wallets WHERE AppUserId = @userId) AS WalletBalance;
END;
");

            // Procedura: przypisuje pacjenta do grupy i loguje zdarzenie
            migrationBuilder.Sql(@"
CREATE OR ALTER PROCEDURE dbo.sp_AddPatientToGroup
    @userId        NVARCHAR(450),
    @groupId       INT,
    @isApproved    BIT = 0
AS
BEGIN
    SET NOCOUNT ON;

    IF EXISTS (
        SELECT 1 FROM PatientGroups
        WHERE AppUserId = @userId AND TherapyGroupId = @groupId
    )
    BEGIN
        SELECT 0 AS Success, N'Użytkownik już należy do tej grupy.' AS Message;
        RETURN;
    END;

    DECLARE @maxPart INT;
    SELECT @maxPart = MaxParticipants FROM TherapyGroups WHERE Id = @groupId;

    DECLARE @currentCount INT;
    SELECT @currentCount = COUNT(*) FROM PatientGroups WHERE TherapyGroupId = @groupId;

    IF @currentCount >= @maxPart
    BEGIN
        SELECT 0 AS Success, N'Grupa jest pełna.' AS Message;
        RETURN;
    END;

    INSERT INTO PatientGroups (AppUserId, TherapyGroupId, JoinedDate, IsApproved)
    VALUES (@userId, @groupId, GETDATE(), @isApproved);

    SELECT 1 AS Success, N'Użytkownik dodany do grupy.' AS Message;
END;
");

            // Procedura: kasuje stare, nieodczytane powiadomienia (starsze niż N dni)
            migrationBuilder.Sql(@"
CREATE OR ALTER PROCEDURE dbo.sp_PurgeOldNotifications
    @olderThanDays INT = 90
AS
BEGIN
    SET NOCOUNT ON;

    DECLARE @cutoff DATETIME = DATEADD(DAY, -@olderThanDays, GETDATE());

    DELETE FROM Notifications
    WHERE  IsRead = 0
      AND  CreatedAt < @cutoff;

    SELECT @@ROWCOUNT AS DeletedCount;
END;
");

            // ════════════════════════════════════════════════════════════════
            //  4. TRIGGERY
            // ════════════════════════════════════════════════════════════════

            // Trigger 1: po dodaniu użytkownika Identity → utwórz portfel automatycznie
            migrationBuilder.Sql(@"
CREATE OR ALTER TRIGGER dbo.trg_AfterInsertAspNetUser
ON AspNetUsers
AFTER INSERT
AS
BEGIN
    SET NOCOUNT ON;

    INSERT INTO Wallets (Id, AppUserId, Balance)
    SELECT NEWID(), i.Id, 0
    FROM   inserted i
    WHERE  NOT EXISTS (
        SELECT 1 FROM Wallets w WHERE w.AppUserId = i.Id
    );
END;
");

            // Trigger 2: po zmianie statusu konsultacji na 'Zakończona' (Id=2)
            //            → utwórz powiadomienie dla pacjenta
            migrationBuilder.Sql(@"
CREATE OR ALTER TRIGGER dbo.trg_AfterConsultationStatusChange
ON Consultations
AFTER UPDATE
AS
BEGIN
    SET NOCOUNT ON;

    -- Tylko gdy StatusId zmienił się NA 2 (Zakończona)
    IF NOT UPDATE(StatusId) RETURN;

    INSERT INTO Notifications (Id, AppUserId, Message, IsRead, CreatedAt)
    SELECT
        NEWID(),
        i.PatientId,
        N'Twoja konsultacja z ' + CONVERT(NVARCHAR, i.StartTime, 120) + N' została oznaczona jako zakończona.',
        0,
        GETDATE()
    FROM inserted  i
    JOIN deleted   d ON i.Id = d.Id
    WHERE i.StatusId = 2
      AND d.StatusId <> 2;
END;
");

            // Trigger 3: blokada usunięcia grupy, jeśli ma aktywnych pacjentów
            migrationBuilder.Sql(@"
CREATE OR ALTER TRIGGER dbo.trg_PreventDeleteNonEmptyGroup
ON TherapyGroups
INSTEAD OF DELETE
AS
BEGIN
    SET NOCOUNT ON;

    IF EXISTS (
        SELECT 1 FROM PatientGroups pg
        JOIN deleted d ON pg.TherapyGroupId = d.Id
    )
    BEGIN
        RAISERROR(N'Nie można usunąć grupy, która ma przypisanych pacjentów.', 16, 1);
        ROLLBACK TRANSACTION;
        RETURN;
    END;

    DELETE FROM TherapyGroups WHERE Id IN (SELECT Id FROM deleted);
END;
");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            // Usuń triggery
            migrationBuilder.Sql("DROP TRIGGER IF EXISTS dbo.trg_AfterInsertAspNetUser;");
            migrationBuilder.Sql("DROP TRIGGER IF EXISTS dbo.trg_AfterConsultationStatusChange;");
            migrationBuilder.Sql("DROP TRIGGER IF EXISTS dbo.trg_PreventDeleteNonEmptyGroup;");

            // Usuń procedury
            migrationBuilder.Sql("DROP PROCEDURE IF EXISTS dbo.sp_GetUserStats;");
            migrationBuilder.Sql("DROP PROCEDURE IF EXISTS dbo.sp_AddPatientToGroup;");
            migrationBuilder.Sql("DROP PROCEDURE IF EXISTS dbo.sp_PurgeOldNotifications;");

            // Usuń funkcje
            migrationBuilder.Sql("DROP FUNCTION IF EXISTS dbo.fn_GetFullName;");
            migrationBuilder.Sql("DROP FUNCTION IF EXISTS dbo.fn_CountUserConsultations;");
            migrationBuilder.Sql("DROP FUNCTION IF EXISTS dbo.fn_AverageWalletBalance;");

            // Usuń tabelę
            migrationBuilder.DropTable(name: "CustomUsers");
        }
    }
}
