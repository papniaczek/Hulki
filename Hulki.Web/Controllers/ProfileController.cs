using Hulki.Web.Data;
using Hulki.Web.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Linq;
using System.Threading.Tasks;

namespace Hulki.Web.Controllers;

[Authorize] // Tylko dla zalogowanych
public class ProfileController : Controller
{
    private readonly ApplicationDbContext _context;
    private readonly UserManager<AppUser> _userManager;

    public ProfileController(ApplicationDbContext context, UserManager<AppUser> userManager)
    {
        _context = context;
        _userManager = userManager;
    }

    public async Task<IActionResult> Index()
    {
        var user = await _userManager.GetUserAsync(User);
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

        ViewBag.User = user;
        ViewBag.Points = wallet?.Balance ?? 0;
        ViewBag.Inventory = inventory;
        ViewBag.Reports = recentReports;

        return View();
    }
}