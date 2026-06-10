using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Hulki.Web.Data;
using Hulki.Web.Models;
using Hulki.Web.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace Hulki.Web.Controllers;

[Authorize(Roles = "Admin,Terapeuta")]
public class AdminController : Controller
{
    private readonly ApplicationDbContext _context;
    private readonly UserManager<AppUser> _userManager;

    private readonly RoleManager<IdentityRole> _roleManager;
    private readonly INotificationService _notificationService;

    public AdminController(ApplicationDbContext context, UserManager<AppUser> userManager, RoleManager<IdentityRole> roleManager, INotificationService notificationService)
    {
        _context = context;
        _userManager = userManager;
        _roleManager = roleManager;
        _notificationService = notificationService;
    }

    // --- DASHBOARD ---
    public async Task<IActionResult> Index()
    {
        var today = DateTime.Today;

        var totalUsers = await _context.Users.CountAsync();
        var totalPatients = totalUsers > 0 ? totalUsers - 1 : 0;

        var reportsToday = await _context
            .DailyReports.Where(r => r.CreatedAt.Date == today)
            .CountAsync();

        var totalGroups = await _context.TherapyGroups.CountAsync();

        var recentReports = await _context
            .DailyReports.Include(r => r.AppUser)
            .Include(r => r.ReportStatus)
            .OrderByDescending(r => r.CreatedAt)
            .Take(5)
            .ToListAsync();

        ViewBag.TotalPatients = totalPatients;
        ViewBag.ReportsToday = reportsToday;
        ViewBag.TotalGroups = totalGroups;
        ViewBag.RecentReports = recentReports;

        return View();
    }

    // ====================================================================
    // ZARZĄDZANIE PACJENTAMI
    // ====================================================================

    // 1. LISTA PACJENTÓW
    [HttpGet]
    public async Task<IActionResult> Patients(string? search)
    {
        var admins = await _userManager.GetUsersInRoleAsync("Admin");
        var adminIds = admins.Select(a => a.Id).ToHashSet();

        var query = _context.Users.AsQueryable().Where(u => !adminIds.Contains(u.Id));

        if (!string.IsNullOrWhiteSpace(search))
        {
            var term = search.Trim().ToLower();
            query = query.Where(u =>
                (u.FirstName != null && u.FirstName.ToLower().Contains(term))
                || (u.LastName != null && u.LastName.ToLower().Contains(term))
                || (u.Email != null && u.Email.ToLower().Contains(term))
            );
        }

        var patients = await query.OrderBy(u => u.LastName).ThenBy(u => u.FirstName).ToListAsync();

        var patientIds = patients.Select(p => p.Id).ToList();

        var wallets = await _context.Wallets
            .Where(w => patientIds.Contains(w.AppUserId))
            .ToDictionaryAsync(w => w.AppUserId, w => w.Balance);

        var reportCounts = await _context.DailyReports
            .Where(r => patientIds.Contains(r.AppUserId))
            .GroupBy(r => r.AppUserId)
            .Select(g => new { UserId = g.Key, Count = g.Count() })
            .ToDictionaryAsync(x => x.UserId, x => x.Count);

        // NOWOŚĆ: sprawdzamy, którzy użytkownicy mają rolę Terapeuta
        var therapists = await _userManager.GetUsersInRoleAsync("Terapeuta");
        var therapistIds = therapists.Select(t => t.Id).ToHashSet();
        // Słownik userId → bool przekazany do widoku
        var isTherapist = patientIds.ToDictionary(id => id, id => therapistIds.Contains(id));

        ViewBag.Wallets = wallets;
        ViewBag.ReportCounts = reportCounts;
        ViewBag.Search = search;
        ViewBag.IsTherapist = isTherapist;   // <-- jedyna nowa linia ViewBag

        return View(patients);
    }

    // 2. SZCZEGÓŁY PACJENTA
    [HttpGet]
    public async Task<IActionResult> PatientDetails(string id)
    {
        if (string.IsNullOrWhiteSpace(id))
            return NotFound();

        var user = await _userManager.FindByIdAsync(id);
        if (user == null)
            return NotFound();

        // Bezpiecznik – nie pokazujemy panelu pacjenta dla konta admina.
        if (await _userManager.IsInRoleAsync(user, "Admin"))
        {
            TempData["ErrorMessage"] = "To konto administratora, nie pacjenta.";
            return RedirectToAction(nameof(Patients));
        }

        var wallet = await _context.Wallets.FirstOrDefaultAsync(w => w.AppUserId == user.Id);

        var reports = await _context
            .DailyReports.Include(r => r.ReportStatus)
            .Where(r => r.AppUserId == user.Id)
            .OrderByDescending(r => r.CreatedAt)
            .Take(20)
            .ToListAsync();

        var inventory = await _context
            .PatientInventories.Include(pi => pi.RewardItem)
                .ThenInclude(ri => ri.ItemRarity)
            .Where(pi => pi.AppUserId == user.Id)
            .ToListAsync();

        var groups = await _context
            .PatientGroups.Include(pg => pg.TherapyGroup)
                .ThenInclude(g => g.TherapyType)
            .Where(pg => pg.AppUserId == user.Id)
            .ToListAsync();

        var transactions =
            wallet != null
                ? await _context
                    .PointTransactions.Where(t => t.WalletId == wallet.Id)
                    .OrderByDescending(t => t.TransactionDate)
                    .Take(15)
                    .ToListAsync()
                : new List<PointTransaction>();

        ViewBag.Patient = user;
        ViewBag.Wallet = wallet;
        ViewBag.Reports = reports;
        ViewBag.Inventory = inventory;
        ViewBag.Groups = groups;
        ViewBag.Transactions = transactions;
        ViewBag.IsLocked = user.LockoutEnd.HasValue && user.LockoutEnd.Value > DateTimeOffset.Now;
        ViewBag.MoodLogs = await _context.MoodLogs
            .Include(m => m.MoodType)
            .Where(m => m.AppUserId == id)
            .OrderByDescending(m => m.Date)
            .Take(5)
            .ToListAsync();
        return View();
    }

    // 3. RĘCZNA KOREKTA PUNKTÓW
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> AdjustPoints(string userId, int amount, string? reason)
    {
        if (string.IsNullOrWhiteSpace(userId) || amount == 0)
        {
            TempData["ErrorMessage"] = "Podaj prawidłową kwotę punktów.";
            return RedirectToAction(nameof(PatientDetails), new { id = userId });
        }

        var wallet = await _context.Wallets.FirstOrDefaultAsync(w => w.AppUserId == userId);

        if (wallet == null)
        {
            wallet = new Wallet
            {
                Id = Guid.NewGuid(),
                AppUserId = userId,
                Balance = 0,
            };
            _context.Wallets.Add(wallet);
        }

        wallet.Balance += amount;
        if (wallet.Balance < 0)
            wallet.Balance = 0;

        _context.PointTransactions.Add(new PointTransaction
        {
            Id = Guid.NewGuid(),
            Amount = amount,
            Description = string.IsNullOrWhiteSpace(reason)
                ? (amount > 0 ? "Korekta terapeuty (+)" : "Korekta terapeuty (-)")
                : $"Korekta terapeuty: {reason}",
            TransactionDate = DateTime.Now,
            WalletId = wallet.Id,
        });

        await _context.SaveChangesAsync();

        await _notificationService.SendNotificationAsync(
            userId,
            $"Twoje saldo punktów zostało zmienione o {amount}. Nowe saldo: {wallet.Balance}."
        );

        TempData["SuccessMessage"] =
            $"Saldo zmienione o {amount} pkt. Nowe saldo: {wallet.Balance}.";

        return RedirectToAction(nameof(PatientDetails), new { id = userId });
    }

    // 4. BLOKADA / ODBLOKOWANIE KONTA
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> ToggleLock(string userId)
    {
        var user = await _userManager.FindByIdAsync(userId);
        if (user == null)
            return NotFound();

        if (await _userManager.IsInRoleAsync(user, "Admin"))
        {
            TempData["ErrorMessage"] = "Nie można zablokować konta administratora.";
            return RedirectToAction(nameof(PatientDetails), new { id = userId });
        }

        bool isCurrentlyLocked =
            user.LockoutEnd.HasValue && user.LockoutEnd.Value > DateTimeOffset.Now;

        if (isCurrentlyLocked)
        {
            await _userManager.SetLockoutEndDateAsync(user, null);
            await _notificationService.SendNotificationAsync(
    userId,
    "Twoje konto zostało odblokowane."
);
            TempData["SuccessMessage"] = "Konto pacjenta zostało odblokowane.";
        }
        else
        {
            await _userManager.SetLockoutEnabledAsync(user, true);
            await _userManager.SetLockoutEndDateAsync(user, DateTimeOffset.UtcNow.AddYears(100));
            await _notificationService.SendNotificationAsync(
    userId,
    "Twoje konto zostało zablokowane przez administratora."
);
            TempData["SuccessMessage"] = "Konto pacjenta zostało zablokowane.";
        }

        return RedirectToAction(nameof(PatientDetails), new { id = userId });
    }

    // 5. INFORMACJE O WPISIE
    [HttpGet]
    public async Task<IActionResult> ReportDetails(Guid id)
    {
        var report = await _context
            .DailyReports.Include(r => r.AppUser)
            .Include(r => r.ReportStatus)
            .Include(r => r.ReportAttachments)
            .FirstOrDefaultAsync(r => r.Id == id);

        if (report == null)
            return NotFound();
        return View(report);
    }

    // 6. AKCEPTACJA / ODRZUCENIE WPISU
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> ApproveReport(Guid id)
    {
        var report = await _context
            .DailyReports.Include(r => r.ReportStatus)
            .FirstOrDefaultAsync(r => r.Id == id);

        if (report == null)
            return NotFound();

        var approvedStatus = await _context.ReportStatuses.FirstOrDefaultAsync(s =>
            s.Name == "Zatwierdzony"
        );
        if (approvedStatus != null && report.ReportStatusId != approvedStatus.Id)
        {
            report.ReportStatusId = approvedStatus.Id;
            await _context.SaveChangesAsync();

            await _notificationService.SendNotificationAsync(
                report.AppUserId,
                "Twój wpis dzienniczka został zatwierdzony!"
            );
            TempData["SuccessMessage"] = "Wpis został zatwierdzony.";
        }

        return RedirectToAction("ReportDetails", new { id = id });
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> RejectReport(Guid id)
    {
        var report = await _context
            .DailyReports.Include(r => r.AppUser)
            .Include(r => r.ReportStatus)
            .Include(r => r.ReportAttachments)
            .FirstOrDefaultAsync(r => r.Id == id);

        if (report == null)
            return NotFound();

        var rejectedStatus = await _context.ReportStatuses.FirstOrDefaultAsync(s =>
            s.Name == "Odrzucony"
        );

        if (rejectedStatus != null && report.ReportStatusId != rejectedStatus.Id)
        {
            report.ReportStatusId = rejectedStatus.Id;

            bool hasAttachment = report.ReportAttachments != null && report.ReportAttachments.Any();
            int pointsToDeduct = hasAttachment ? 15 : 10;

            var wallet = await _context.Wallets.FirstOrDefaultAsync(w =>
                w.AppUserId == report.AppUserId
            );
            if (wallet != null)
            {
                wallet.Balance -= pointsToDeduct;
                if (wallet.Balance < 0)
                    wallet.Balance = 0;

                _context.PointTransactions.Add(
                    new PointTransaction
                    {
                        Id = Guid.NewGuid(),
                        Amount = -pointsToDeduct,
                        Description = hasAttachment
                            ? "Odrzucenie wpisu w dzienniczku z załącznikiem (cofnięcie punktów)"
                            : "Odrzucenie wpisu w dzienniczku (cofnięcie punktów)",
                        TransactionDate = DateTime.Now,
                        WalletId = wallet.Id,
                    }
                );
            }

            await _context.SaveChangesAsync();

            await _notificationService.SendNotificationAsync(
                report.AppUserId,
                $"Twój wpis został odrzucony. Utracono {pointsToDeduct} punktów."
            );
            TempData["ErrorMessage"] =
                $"Wpis został odrzucony. Odebrano {pointsToDeduct} pkt z konta pacjenta.";
        }

        return RedirectToAction("ReportDetails", new { id = id });
    }

    /// <summary>
    /// POST /Admin/ToggleTherapist
    /// Nadaje lub odbiera rolę Terapeuta wskazanemu użytkownikowi.
    /// Rola "Terapeuta" jest tworzona automatycznie jeśli nie istnieje.
    /// </summary>
    [HttpPost]
    [ValidateAntiForgeryToken]
    [Authorize(Roles = "Admin")]
    public async Task<IActionResult> ToggleTherapist(string userId, string? returnUrl)
    {
        var user = await _userManager.FindByIdAsync(userId);
        if (user == null)
            return NotFound();

        if (await _userManager.IsInRoleAsync(user, "Admin"))
        {
            TempData["ErrorMessage"] = "Nie można zmieniać roli konta administratora.";
            return Redirect(returnUrl ?? Url.Action(nameof(Patients))!);
        }

        const string role = "Terapeuta";

        if (!await _roleManager.RoleExistsAsync(role))
            await _roleManager.CreateAsync(new IdentityRole(role));

        var isTherapist = await _userManager.IsInRoleAsync(user, role);

        if (isTherapist)
        {
            await _userManager.RemoveFromRoleAsync(user, role);

            user.IsTherapist = false;
            await _userManager.UpdateAsync(user);
            await _notificationService.SendNotificationAsync(
                userId,
                "Twoja rola terapeuty została odebrana."
            );
            TempData["SuccessMessage"] =
                $"{user.FirstName} {user.LastName} nie jest już terapeutą.";
        }
        else
        {
            await _userManager.AddToRoleAsync(user, role);

            user.IsTherapist = true;
            await _userManager.UpdateAsync(user);
            await _notificationService.SendNotificationAsync(
                userId,
                "Otrzymałeś rolę terapeuty."
            );
            TempData["SuccessMessage"] =
                $"{user.FirstName} {user.LastName} otrzymał(a) rolę terapeuty.";
        }

        return Redirect(returnUrl ?? Url.Action(nameof(Patients))!);
    }

    // ====================================================================
    // ZARZĄDZANIE PRZEDMIOTAMI W SKLEPIE
    // ====================================================================

    // 1. LISTA PRZEDMIOTÓW
    [HttpGet]
    public async Task<IActionResult> ShopItems()
    {
        var items = await _context
            .RewardItems.Include(r => r.ItemRarity)
            .OrderBy(r => r.ItemRarityId)
            .ThenBy(r => r.Name)
            .ToListAsync();

        ViewBag.Rarities = await _context.ItemRarities.OrderBy(r => r.Name).ToListAsync();
        return View(items);
    }

    // 2. EDYCJA PRZEDMIOTU – GET
    [HttpGet]
    public async Task<IActionResult> EditRewardItem(Guid id)
    {
        var item = await _context
            .RewardItems.Include(r => r.ItemRarity)
            .FirstOrDefaultAsync(r => r.Id == id);

        if (item == null)
            return NotFound();

        ViewBag.Rarities = await _context.ItemRarities.OrderBy(r => r.Name).ToListAsync();
        return View(item);
    }

    // 3. EDYCJA PRZEDMIOTU – POST
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> EditRewardItem(
        Guid id,
        string name,
        string? description,
        int price,
        string? iconPath,
        int itemRarityId
    )
    {
        var item = await _context.RewardItems.FirstOrDefaultAsync(r => r.Id == id);
        if (item == null)
            return NotFound();

        if (string.IsNullOrWhiteSpace(name))
        {
            ModelState.AddModelError("name", "Nazwa nie może być pusta.");
            ViewBag.Rarities = await _context.ItemRarities.OrderBy(r => r.Name).ToListAsync();
            return View(item);
        }

        item.Name = name.Trim();
        item.Description = string.IsNullOrWhiteSpace(description) ? null : description.Trim();
        item.Price = price < 0 ? 0 : price;
        item.IconPath = string.IsNullOrWhiteSpace(iconPath) ? null : iconPath.Trim();
        item.ItemRarityId = itemRarityId;

        await _context.SaveChangesAsync();
        TempData["SuccessMessage"] = $"Zapisano zmiany w przedmiocie: {item.Name}.";
        return RedirectToAction(nameof(ShopItems));
    }

    // 4. SZYBKIE DODANIE Z LISTY
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> CreateRewardItem(
        string name,
        string? description,
        int price,
        string? iconPath,
        int itemRarityId
    )
    {
        if (string.IsNullOrWhiteSpace(name))
        {
            TempData["ErrorMessage"] = "Nazwa przedmiotu jest wymagana.";
            return RedirectToAction(nameof(ShopItems));
        }

        _context.RewardItems.Add(
            new RewardItem
            {
                Id = Guid.NewGuid(),
                Name = name.Trim(),
                Description = string.IsNullOrWhiteSpace(description) ? null : description.Trim(),
                Price = price < 0 ? 0 : price,
                IconPath = string.IsNullOrWhiteSpace(iconPath) ? null : iconPath.Trim(),
                ItemRarityId = itemRarityId,
            }
        );

        await _context.SaveChangesAsync();
        TempData["SuccessMessage"] = $"Dodano przedmiot: {name}.";
        return RedirectToAction(nameof(ShopItems));
    }

    // 5. USUWANIE PRZEDMIOTU
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> DeleteRewardItem(Guid id)
    {
        var item = await _context.RewardItems.FindAsync(id);
        if (item == null)
            return NotFound();

        var ownedEntries = _context.PatientInventories.Where(pi => pi.RewardItemId == id);
        _context.PatientInventories.RemoveRange(ownedEntries);

        _context.RewardItems.Remove(item);
        await _context.SaveChangesAsync();

        TempData["SuccessMessage"] = $"Usunięto przedmiot: {item.Name}.";
        return RedirectToAction(nameof(ShopItems));
    }
}
