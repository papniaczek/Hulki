using System.Threading.Tasks;
using Hulki.Web.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Hulki.Web.Controllers;

/// <summary>
/// Przykładowy kontroler demonstrujący wywoływanie procedur składowanych,
/// funkcji SQL i triggerów z poziomu C# przez SqlObjectsService.
/// 
/// Trasy:
///   GET  /SqlDemo/stats/{userId}         → sp_GetUserStats
///   POST /SqlDemo/add-to-group           → sp_AddPatientToGroup
///   POST /SqlDemo/purge-notifications    → sp_PurgeOldNotifications
///   GET  /SqlDemo/fullname/{userId}      → fn_GetFullName
///   GET  /SqlDemo/avg-balance            → fn_AverageWalletBalance
/// </summary>
[Authorize(Roles = "Admin")]
public class SqlDemoController : Controller
{
    private readonly SqlObjectsService _sql;

    public SqlDemoController(SqlObjectsService sql) => _sql = sql;

    // ── PROCEDURY ────────────────────────────────────────────────────────

    /// <summary>Wywołuje sp_GetUserStats – statystyki użytkownika z bazy.</summary>
    [HttpGet]
    public async Task<IActionResult> Stats(string userId)
    {
        var stats = await _sql.GetUserStatsAsync(userId);
        if (stats is null) return NotFound("Nie znaleziono użytkownika.");
        return Json(stats);
    }

    /// <summary>Wywołuje sp_AddPatientToGroup – dodaje pacjenta do grupy.</summary>
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> AddToGroup(string userId, int groupId)
    {
        var result = await _sql.AddPatientToGroupAsync(userId, groupId, isApproved: true);
        return Json(new { result.Success, result.Message });
    }

    /// <summary>Wywołuje sp_PurgeOldNotifications – czyści stare powiadomienia.</summary>
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> PurgeNotifications(int days = 90)
    {
        var deleted = await _sql.PurgeOldNotificationsAsync(days);
        return Json(new { deleted, message = $"Usunięto {deleted} powiadomień starszych niż {days} dni." });
    }

    /// <summary>Wywołuje sp_PurchaseRewardItem – zakup przedmiotu ze sklepu.</summary>
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> PurchaseItem(string userId, Guid rewardItemId)
    {
        var result = await _sql.PurchaseRewardItemAsync(userId, rewardItemId);
        return Json(new { result.Success, result.Message });
    }

    /// <summary>Wywołuje sp_AwardBadge – przyznaje odznakę użytkownikowi.</summary>
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> AwardBadge(string userId, Guid badgeId)
    {
        var result = await _sql.AwardBadgeAsync(userId, badgeId);
        return Json(new { result.Success, result.Message });
    }

    // ── FUNKCJE SQL ──────────────────────────────────────────────────────

    /// <summary>Wywołuje fn_GetFullName – imię i nazwisko z bazy.</summary>
    [HttpGet]
    public async Task<IActionResult> FullName(string userId)
    {
        var name = await _sql.GetFullNameAsync(userId);
        return Json(new { fullName = name });
    }

    /// <summary>Wywołuje fn_AverageWalletBalance – średnie saldo portfela.</summary>
    [HttpGet]
    public async Task<IActionResult> AverageBalance()
    {
        var avg = await _sql.GetAverageWalletBalanceAsync();
        return Json(new { averageBalance = avg });
    }

    /// <summary>Wywołuje fn_GetTotalEarnedPoints – suma zdobytych punktów.</summary>
    [HttpGet]
    public async Task<IActionResult> TotalEarnedPoints(string userId)
    {
        var total = await _sql.GetTotalEarnedPointsAsync(userId);
        return Json(new { totalEarnedPoints = total });
    }

    /// <summary>Wywołuje fn_CountUserBadges – liczba zdobytych odznak.</summary>
    [HttpGet]
    public async Task<IActionResult> BadgeCount(string userId)
    {
        var count = await _sql.CountUserBadgesAsync(userId);
        return Json(new { badgeCount = count });
    }

    // ── TRIGGER – demonstracja pośrednia ─────────────────────────────────
    // Triggery działają automatycznie w SQL Server. Przykłady działania:
    //
    // trg_AfterInsertAspNetUser        → AFTER INSERT na AspNetUsers,
    //                                     tworzy portfel nowego użytkownika.
    //
    // trg_AfterConsultationStatusChange → AFTER UPDATE na Consultations,
    //                                      tworzy powiadomienie gdy status → 2 (Zakończona).
    //
    // trg_AuditTherapyGroupDelete      → AFTER DELETE na TherapyGroups,
    //                                     zapisuje log usunięcia do TherapyGroupDeletionLogs
    //                                     (INSTEAD OF nie było możliwe – tabela ma wchodzące
    //                                      kaskady FK z PatientGroups/GroupMessages/itd.).
    //
    // trg_AfterPointTransactionInsert  → AFTER INSERT na PointTransactions,
    //                                     aktualizuje saldo portfela.
    //
    // trg_PreventNegativeWalletBalance → AFTER UPDATE na Wallets,
    //                                     blokuje zejście salda portfela pod zero.
    [HttpGet]
    public IActionResult TriggerInfo()
    {
        return Json(new
        {
            triggers = new[]
            {
                new { name = "trg_AfterInsertAspNetUser",         table = "AspNetUsers",       event_ = "AFTER INSERT",  effect = "Tworzy portfel nowego użytkownika" },
                new { name = "trg_AfterConsultationStatusChange", table = "Consultations",     event_ = "AFTER UPDATE",  effect = "Tworzy powiadomienie po zakończeniu konsultacji" },
                new { name = "trg_AuditTherapyGroupDelete",       table = "TherapyGroups",     event_ = "AFTER DELETE",  effect = "Zapisuje log usunięcia grupy terapeutycznej" },
                new { name = "trg_AfterPointTransactionInsert",   table = "PointTransactions", event_ = "AFTER INSERT",  effect = "Aktualizuje saldo portfela" },
                new { name = "trg_PreventNegativeWalletBalance",  table = "Wallets",           event_ = "AFTER UPDATE",  effect = "Blokuje ujemne saldo portfela" }
            }
        });
    }
}