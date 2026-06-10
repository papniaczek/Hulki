using Hulki.Web.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Hulki.Web.Models;

namespace Hulki.Web.Controllers;

[Authorize]
public class ConsultationController : Controller
{
    private readonly IConsultationService _consultationService;
    private readonly UserManager<AppUser> _userManager;

    public ConsultationController(IConsultationService consultationService, UserManager<AppUser> userManager)
    {
        _consultationService = consultationService;
        _userManager = userManager;
    }

    public async Task<IActionResult> Index()
    {
        var user = await _userManager.GetUserAsync(User);
        if (user == null) return NotFound();

        // Tu musisz dodać logikę sprawdzającą, czy to pacjent czy terapeuta
        var consultations = await _consultationService.GetPatientConsultationsAsync(user.Id);

        return View(consultations);
    }
}