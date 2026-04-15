using System.Diagnostics;
using Microsoft.AspNetCore.Mvc;
using Hulki.Web.Models;
using Hulki.Web.Data;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using System.Threading.Tasks;

namespace Hulki.Web.Controllers;

public class HomeController : Controller
{
    private readonly ApplicationDbContext _context;
    private readonly UserManager<AppUser> _userManager;

    public HomeController(ApplicationDbContext context, UserManager<AppUser> userManager)
    {
        _context = context;
        _userManager = userManager;
    }

    public async Task<IActionResult> Index()
    {
        if (User.Identity.IsAuthenticated)
        {
            // Znajdź zalogowanego użytkownika
            var user = await _userManager.GetUserAsync(User);
                
            // Pobierz jego portfel z bazy
            var wallet = await _context.Wallets.FirstOrDefaultAsync(w => w.AppUserId == user.Id);
                
            // Przekaż liczbę punktów do widoku (ViewBag to taki prosty transporter danych)
            ViewBag.Points = wallet?.Balance ?? 0;
        }

        return View();
    }

    public IActionResult Privacy()
    {
        return View();
    }
}