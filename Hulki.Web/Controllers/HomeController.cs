using System.Diagnostics;
using System.Threading.Tasks;
using Hulki.Web.Data;
using Hulki.Web.Models;
using Hulki.Web.Services;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace Hulki.Web.Controllers;

public class HomeController : Controller
{
    private readonly ApplicationDbContext _context;
    private readonly UserManager<AppUser> _userManager;
    private readonly IQuoteService _quoteService;

    public HomeController(ApplicationDbContext context, UserManager<AppUser> userManager, IQuoteService quoteService)
    {
        _context = context;
        _userManager = userManager;
        _quoteService = quoteService;
    }

    public async Task<IActionResult> Index()
    {
        if (User.Identity?.IsAuthenticated == true)
        {
            var user = await _userManager.GetUserAsync(User);
            var wallet = await _context.Wallets.FirstOrDefaultAsync(w => w.AppUserId == user!.Id);
            ViewBag.Points = wallet?.Balance ?? 0;
        }

        // Cytat pobieramy tutaj, żeby strona renderowała się już z treścią
        // (serwis sam obsługuje fallback w razie awarii zewnętrznego API).
        ViewBag.Quote = await _quoteService.GetRandomQuoteAsync();

        return View();
    }

    public IActionResult Privacy()
    {
        return View();
    }
}
