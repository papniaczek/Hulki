using Hulki.Web.Data;
using Hulki.Web.Models;
using Hulki.Web.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System;
using System.Linq;
using System.Threading.Tasks;

namespace Hulki.Web.Controllers;

[Authorize]
public class ProfileController : Controller
{
    private readonly ApplicationDbContext _context;
    private readonly UserManager<AppUser> _userManager;
    private readonly IBadgeService _badgeService;

    public ProfileController(
        ApplicationDbContext context,
        UserManager<AppUser> userManager,
        IBadgeService badgeService)
    {
        _context = context;
        _userManager = userManager;
        _badgeService = badgeService;
    }

    public async Task<IActionResult> Index()
    {
        var user = await _userManager.GetUserAsync(User);

        if (user == null) return Challenge();

        var wallet = await _context.Wallets.FirstOrDefaultAsync(w => w.AppUserId == user.Id);

        var inventory = await _context.PatientInventories
            .Include(pi => pi.RewardItem)
            .ThenInclude(ri => ri.ItemRarity)
            .Where(pi => pi.AppUserId == user.Id)
            .Select(pi => pi.RewardItem)
            .ToListAsync();

        var recentReports = await _context.DailyReports
            .Include(dr => dr.ReportStatus)
            .Where(dr => dr.AppUserId == user.Id)
            .OrderByDescending(dr => dr.CreatedAt)
            .Take(5)
            .ToListAsync();

        var badges = await _badgeService.GetUserBadgesAsync(user.Id);

        ViewBag.User = user;
        ViewBag.Points = wallet?.Balance ?? 0;
        ViewBag.Inventory = inventory;
        ViewBag.Reports = recentReports;
        ViewBag.Badges = badges;

        return View();
    }

    [HttpGet]
    public async Task<IActionResult> DebugBadges()
    {
        var user = await _userManager.GetUserAsync(User);
        if (user == null) return Unauthorized();

        // 1. Awaryjne wstrzyknięcie odznak do bazy, jeśli ich tam nie ma
        if (!await _context.AchievementBadges.AnyAsync())
        {
            _context.AchievementBadges.AddRange(
                new AchievementBadge
                {
                    Id = Guid.NewGuid(),
                    Name = "Pierwszy krok",
                    Description = "Ukończono pierwszy cel terapeutyczny.",
                    IconPath = "bi-star-fill",
                    ConditionType = "GoalsCompleted",
                    ConditionValue = 1
                },
                new AchievementBadge
                {
                    Id = Guid.NewGuid(),
                    Name = "Weteran",
                    Description = "Ukończono 5 celów terapeutycznych.",
                    IconPath = "bi-trophy-fill",
                    ConditionType = "GoalsCompleted",
                    ConditionValue = 5
                }
            );
            await _context.SaveChangesAsync();
        }

        await _badgeService.CheckAndAwardBadgesAsync(user.Id);

        return RedirectToAction(nameof(Index));
    }
}