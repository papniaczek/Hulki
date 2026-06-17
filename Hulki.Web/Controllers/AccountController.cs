using System.Threading.Tasks;
using Hulki.Web.Data;
using Hulki.Web.Models;
using Hulki.Web.Services;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace Hulki.Web.Controllers;

public class AccountController : Controller
{
    private readonly UserManager<AppUser> _userManager;
    private readonly SignInManager<AppUser> _signInManager;
    private readonly ApplicationDbContext _context;

    public AccountController(
        UserManager<AppUser> userManager,
        SignInManager<AppUser> signInManager,
        ApplicationDbContext context
    )
    {
        _userManager = userManager;
        _signInManager = signInManager;
        _context = context;
    }

    [HttpGet]
    public IActionResult Register() => View();

    [HttpPost]
    public async Task<IActionResult> Register(
        string email,
        string password,
        string firstName,
        string lastName
    )
    {
        var user = new AppUser
        {
            UserName = email,
            Email = email,
            FirstName = firstName,
            LastName = lastName,
        };
        var result = await _userManager.CreateAsync(user, password);

        if (result.Succeeded)
        {
            var wallet = new Wallet { AppUserId = user.Id, Balance = 0 };
            _context.Wallets.Add(wallet);
            
            if (!await _context.CustomUsers.AnyAsync(u => u.Email == email))
            {
                _context.CustomUsers.Add(new CustomUser
                {
                    FirstName    = firstName,
                    LastName     = lastName,
                    Email        = email,
                    PasswordHash = CustomPasswordHasher.Hash(password),
                    IsTherapist  = false,
                    AspNetUserId = user.Id
                });
            }

            await _context.SaveChangesAsync();

            await _signInManager.SignInAsync(user, isPersistent: false);
            return RedirectToAction("Index", "Home");
        }

        foreach (var error in result.Errors)
            ModelState.AddModelError(string.Empty, error.Description);

        return View();
    }

    [HttpGet]
    public async Task<IActionResult> CheckEmailExists(string email)
    {
        var user = await _userManager.FindByEmailAsync(email);
        return Json(new { exists = user != null });
    }

    [HttpGet]
    public IActionResult Login() => View();

    [HttpPost]
    public async Task<IActionResult> Login(string email, string password)
    {
        // weryfikacja hasła
        var customUser = await _context.CustomUsers
            .FirstOrDefaultAsync(u => u.Email == email);

        if (customUser is null || !CustomPasswordHasher.Verify(password, customUser.PasswordHash))
        {
            ModelState.AddModelError(string.Empty, "Nieprawidłowy email lub hasło.");
            return View();
        }
        
        var appUser = await _userManager.FindByEmailAsync(email);
        if (appUser is null)
        {
            ModelState.AddModelError(string.Empty, "Konto nie istnieje w systemie logowania.");
            return View();
        }

        await _signInManager.SignInAsync(appUser, isPersistent: false);
        return RedirectToAction("Index", "Home");
    }

    [HttpPost]
    public async Task<IActionResult> Logout()
    {
        await _signInManager.SignOutAsync();
        return RedirectToAction("Index", "Home");
    }
}