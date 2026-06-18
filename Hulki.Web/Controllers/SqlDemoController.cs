using System.Threading.Tasks;
using Hulki.Web.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Hulki.Web.Controllers;

[Authorize(Roles = "Admin")]
public class SqlDemoController : Controller
{
    private readonly SqlObjectsService _sql;

    public SqlDemoController(SqlObjectsService sql) => _sql = sql;

    // PROCEDURY

    // sp_GetUserStats – statystyki użytkownika z bazy
    [HttpGet]
    public async Task<IActionResult> Stats(string userId)
    {
        var stats = await _sql.GetUserStatsAsync(userId);
        if (stats is null) return NotFound("Nie znaleziono użytkownika.");
        return Json(stats);
    }

    // sp_AddPatientToGroup – dodaje pacjenta do grupy
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> AddToGroup(string userId, int groupId)
    {
        var result = await _sql.AddPatientToGroupAsync(userId, groupId, isApproved: true);
        return Json(new { result.Success, result.Message });
    }

    // sp_PurgeOldNotifications – czyści stare powiadomienia
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> PurgeNotifications(int days = 90)
    {
        var deleted = await _sql.PurgeOldNotificationsAsync(days);
        return Json(new { deleted, message = $"Usunięto {deleted} powiadomień starszych niż {days} dni." });
    }

    // sp_PurchaseRewardItem – zakup przedmiotu ze sklepu
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> PurchaseItem(string userId, Guid rewardItemId)
    {
        var result = await _sql.PurchaseRewardItemAsync(userId, rewardItemId);
        return Json(new { result.Success, result.Message });
    }

    // sp_AwardBadge – przyznaje odznakę użytkownikowi
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> AwardBadge(string userId, Guid badgeId)
    {
        var result = await _sql.AwardBadgeAsync(userId, badgeId);
        return Json(new { result.Success, result.Message });
    }

    // FUNKCJE

    // fn_GetFullName – imię i nazwisko z bazy
    [HttpGet]
    public async Task<IActionResult> FullName(string userId)
    {
        var name = await _sql.GetFullNameAsync(userId);
        return Json(new { fullName = name });
    }

    // fn_AverageWalletBalance – średnie saldo portfela
    [HttpGet]
    public async Task<IActionResult> AverageBalance()
    {
        var avg = await _sql.GetAverageWalletBalanceAsync();
        return Json(new { averageBalance = avg });
    }

    // fn_GetTotalEarnedPoints – suma zdobytych punktów
    [HttpGet]
    public async Task<IActionResult> TotalEarnedPoints(string userId)
    {
        var total = await _sql.GetTotalEarnedPointsAsync(userId);
        return Json(new { totalEarnedPoints = total });
    }

    // fn_CountUserBadges – liczba zdobytych odznak
    [HttpGet]
    public async Task<IActionResult> BadgeCount(string userId)
    {
        var count = await _sql.CountUserBadgesAsync(userId);
        return Json(new { badgeCount = count });
    }

    // TRIGGERY
    // trg_AfterInsertAspNetUser - tworzy portfel nowego użytkownika.
    // trg_AfterConsultationStatusChange - tworzy powiadomienie gdy status = Zakończona
    // trg_AuditTherapyGroupDelete - zapisuje log usunięcia do TherapyGroupDeletionLogs
    // trg_AfterPointTransactionInsert - aktualizuje saldo portfela
    // trg_PreventNegativeWalletBalance - blokuje zejście salda portfela pod zero
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