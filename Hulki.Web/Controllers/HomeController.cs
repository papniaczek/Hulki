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
        // 1. Sprawdzamy czy użytkownik jest zalogowany
        if (User.Identity?.IsAuthenticated == true)
        {
            var user = await _userManager.GetUserAsync(User);

            // 2. Bezpieczne sprawdzenie czy user faktycznie został pobrany (zabezpieczenie przed null)
            if (user != null)
            {
                var wallet = await _context.Wallets.FirstOrDefaultAsync(w => w.AppUserId == user.Id);
                ViewBag.Points = wallet?.Balance ?? 0;
            }
            else
            {
                // Jeśli user jest null mimo IsAuthenticated, wymuszamy wylogowanie/bezpieczne zero
                ViewBag.Points = 0;
            }
        }
        else
        {
            ViewBag.Points = 0;
        }

        // Cytat zawsze się pobierze
        ViewBag.Quote = await _quoteService.GetRandomQuoteAsync();

        return View();
    }

    public IActionResult Privacy()
    {
        return View();
    }
}
