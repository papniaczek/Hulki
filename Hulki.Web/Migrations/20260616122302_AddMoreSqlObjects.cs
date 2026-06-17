using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Hulki.Web.Migrations
{
    /// <inheritdoc />
    public partial class AddMoreSqlObjects : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            // ════════════════════════════════════════════════════════════════
            //  0. POPRAWKA BŁĘDU w triggerze z poprzedniej migracji
            //     (Notifications.Message nie istnieje – kolumna nazywa się Content)
            // ════════════════════════════════════════════════════════════════
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

            // ════════════════════════════════════════════════════════════════
            //  1. NOWE FUNKCJE (dobijamy do 5)
            // ════════════════════════════════════════════════════════════════

            // Funkcja 4: liczy punkty zdobyte przez pacjenta (suma transakcji > 0)
            migrationBuilder.Sql(@"
CREATE OR ALTER FUNCTION dbo.fn_GetTotalEarnedPoints(@userId NVARCHAR(450))
RETURNS INT
AS
BEGIN
    DECLARE @total INT;

    SELECT @total = SUM(pt.Amount)
    FROM   PointTransactions pt
    JOIN   Wallets w ON pt.WalletId = w.Id
    WHERE  w.AppUserId = @userId
      AND  pt.Amount > 0;

    RETURN ISNULL(@total, 0);
END;
");

            // Funkcja 5: zwraca liczbę odznak (badge) zdobytych przez użytkownika
            migrationBuilder.Sql(@"
CREATE OR ALTER FUNCTION dbo.fn_CountUserBadges(@userId NVARCHAR(450))
RETURNS INT
AS
BEGIN
    DECLARE @cnt INT;

    SELECT @cnt = COUNT(*)
    FROM   UserBadges
    WHERE  AppUserId = @userId;

    RETURN ISNULL(@cnt, 0);
END;
");

            // ════════════════════════════════════════════════════════════════
            //  2. NOWE PROCEDURY (dobijamy do 5)
            // ════════════════════════════════════════════════════════════════

            // Procedura 4: kupno przedmiotu ze sklepu – sprawdza saldo,
            //              odejmuje punkty, dodaje przedmiot, zapisuje transakcję
            migrationBuilder.Sql(@"
CREATE OR ALTER PROCEDURE dbo.sp_PurchaseRewardItem
    @userId       NVARCHAR(450),
    @rewardItemId UNIQUEIDENTIFIER
AS
BEGIN
    SET NOCOUNT ON;

    DECLARE @price   INT;
    DECLARE @balance INT;
    DECLARE @walletId UNIQUEIDENTIFIER;

    SELECT @price = Price FROM RewardItems WHERE Id = @rewardItemId;
    IF @price IS NULL
    BEGIN
        SELECT 0 AS Success, N'Przedmiot nie istnieje.' AS Message;
        RETURN;
    END;

    SELECT @walletId = Id, @balance = Balance
    FROM   Wallets
    WHERE  AppUserId = @userId;

    IF @balance IS NULL OR @balance < @price
    BEGIN
        SELECT 0 AS Success, N'Niewystarczające saldo.' AS Message;
        RETURN;
    END;

    IF EXISTS (SELECT 1 FROM PatientInventories WHERE AppUserId = @userId AND RewardItemId = @rewardItemId)
    BEGIN
        SELECT 0 AS Success, N'Przedmiot już posiadany.' AS Message;
        RETURN;
    END;

    BEGIN TRANSACTION;

    -- Saldo portfela aktualizowane automatycznie przez trigger
    -- trg_AfterPointTransactionInsert po wstawieniu transakcji poniżej.
    INSERT INTO PointTransactions (Id, Amount, Description, TransactionDate, WalletId)
    VALUES (NEWID(), -@price, N'Zakup przedmiotu ze sklepu', GETDATE(), @walletId);

    INSERT INTO PatientInventories (AppUserId, RewardItemId, AcquiredDate)
    VALUES (@userId, @rewardItemId, GETDATE());

    COMMIT TRANSACTION;

    SELECT 1 AS Success, N'Zakup zakończony sukcesem.' AS Message;
END;
");

            // Procedura 5: przyznaje odznakę użytkownikowi, jeśli jeszcze jej nie ma
            migrationBuilder.Sql(@"
CREATE OR ALTER PROCEDURE dbo.sp_AwardBadge
    @userId  NVARCHAR(450),
    @badgeId UNIQUEIDENTIFIER
AS
BEGIN
    SET NOCOUNT ON;

    IF EXISTS (SELECT 1 FROM UserBadges WHERE AppUserId = @userId AND BadgeId = @badgeId)
    BEGIN
        SELECT 0 AS Success, N'Odznaka już przyznana.' AS Message;
        RETURN;
    END;

    INSERT INTO UserBadges (Id, AppUserId, BadgeId, EarnedAt)
    VALUES (NEWID(), @userId, @badgeId, GETDATE());

    INSERT INTO Notifications (Id, AppUserId, Content, IsRead, CreatedAt)
    SELECT NEWID(), @userId, N'Zdobyto nową odznakę: ' + Name, 0, GETDATE()
    FROM   AchievementBadges
    WHERE  Id = @badgeId;

    SELECT 1 AS Success, N'Odznaka przyznana.' AS Message;
END;
");

            // ════════════════════════════════════════════════════════════════
            //  3. NOWE TRIGGERY
            // ════════════════════════════════════════════════════════════════

            // Trigger 4: po wstawieniu transakcji punktowej -> aktualizuj saldo portfela
            //            (dodatkowa warstwa konsystencji, niezależna od kodu C#)
            migrationBuilder.Sql(@"
CREATE OR ALTER TRIGGER dbo.trg_AfterPointTransactionInsert
ON PointTransactions
AFTER INSERT
AS
BEGIN
    SET NOCOUNT ON;

    UPDATE w
    SET    w.Balance = w.Balance + i.Amount
    FROM   Wallets w
    JOIN   inserted i ON w.Id = i.WalletId;
END;
");

            // Trigger 5: zabezpieczenie przed ujemnym saldem portfela
            migrationBuilder.Sql(@"
CREATE OR ALTER TRIGGER dbo.trg_PreventNegativeWalletBalance
ON Wallets
AFTER UPDATE
AS
BEGIN
    SET NOCOUNT ON;

    IF EXISTS (SELECT 1 FROM inserted WHERE Balance < 0)
    BEGIN
        RAISERROR(N'Saldo portfela nie może być ujemne.', 16, 1);
        ROLLBACK TRANSACTION;
    END;
END;
");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.Sql("DROP TRIGGER IF EXISTS dbo.trg_AfterPointTransactionInsert;");
            migrationBuilder.Sql("DROP TRIGGER IF EXISTS dbo.trg_PreventNegativeWalletBalance;");

            migrationBuilder.Sql("DROP PROCEDURE IF EXISTS dbo.sp_PurchaseRewardItem;");
            migrationBuilder.Sql("DROP PROCEDURE IF EXISTS dbo.sp_AwardBadge;");

            migrationBuilder.Sql("DROP FUNCTION IF EXISTS dbo.fn_GetTotalEarnedPoints;");
            migrationBuilder.Sql("DROP FUNCTION IF EXISTS dbo.fn_CountUserBadges;");

            // Przywróć starą (błędną) wersję triggera nie ma sensu – zostawiamy poprawioną.
        }
    }
}