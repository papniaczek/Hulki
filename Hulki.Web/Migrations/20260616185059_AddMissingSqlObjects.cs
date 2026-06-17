using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Hulki.Web.Migrations
{
    /// <summary>
    /// Migracja naprawcza – odtwarza funkcje, procedury i triggery,
    /// które nie zostały zapisane w migracji "addcustomers"
    /// (sprawdzone: brakowało ich w bazie, mimo że migracja byla oznaczona jako zastosowana).
    /// Wszystkie polecenia używają CREATE OR ALTER, więc są bezpieczne nawet
    /// jeśli część obiektów już istnieje.
    /// </summary>
    public partial class AddMissingSqlObjects : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            // ════════════════════════════════════════════════════════════════
            //  FUNKCJE
            // ════════════════════════════════════════════════════════════════

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
            //  PROCEDURY
            // ════════════════════════════════════════════════════════════════

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
            //  TRIGGERY
            // ════════════════════════════════════════════════════════════════

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

            migrationBuilder.Sql(@"
CREATE OR ALTER TRIGGER dbo.trg_AfterConsultationStatusChange
ON Consultations
AFTER UPDATE
AS
BEGIN
    SET NOCOUNT ON;

    IF NOT UPDATE(StatusId) RETURN;

    INSERT INTO Notifications (Id, AppUserId, Content, IsRead, CreatedAt)
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
            // trg_PreventDeleteNonEmptyGroup nie jest tworzony w tej migracji –
            // SQL Server nie pozwala na INSTEAD OF DELETE na tabeli z wchodzącą
            // kaskadą FK (PatientGroups, GroupMessages, GroupQuests, GroupResources).
            // Zobacz migrację FixTherapyGroupDeleteTrigger – tam jest trigger
            // AFTER DELETE (audyt usunięć), który jest z kaskadami kompatybilny.
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.Sql("DROP TRIGGER IF EXISTS dbo.trg_AfterInsertAspNetUser;");
            migrationBuilder.Sql("DROP TRIGGER IF EXISTS dbo.trg_AfterConsultationStatusChange;");

            migrationBuilder.Sql("DROP PROCEDURE IF EXISTS dbo.sp_GetUserStats;");
            migrationBuilder.Sql("DROP PROCEDURE IF EXISTS dbo.sp_AddPatientToGroup;");
            migrationBuilder.Sql("DROP PROCEDURE IF EXISTS dbo.sp_PurgeOldNotifications;");

            migrationBuilder.Sql("DROP FUNCTION IF EXISTS dbo.fn_GetFullName;");
            migrationBuilder.Sql("DROP FUNCTION IF EXISTS dbo.fn_CountUserConsultations;");
            migrationBuilder.Sql("DROP FUNCTION IF EXISTS dbo.fn_AverageWalletBalance;");
        }
    }
}