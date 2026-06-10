using Hulki.Web.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using Hulki.Web.Models;
using Hulki.Web.Models.Dto;

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

        var consultations = await _consultationService.GetPatientConsultationsAsync(user.Id);
        return View(consultations);
    }

    [HttpGet]
    public async Task<IActionResult> Create()
    {
        var user = await _userManager.GetUserAsync(User);
        if (user == null) return NotFound();

        if (user.IsTherapist)
        {
            var patients = await _userManager.Users
                .Where(u => !u.IsTherapist)
                .ToListAsync();

            ViewBag.TargetUsers = new SelectList(
    patients.Select(u => new
    {
        u.Id,
        FullName = u.FirstName + " " + u.LastName
    }),
    "Id",
    "FullName"
);
            ViewBag.RoleLabel = "Wybierz Pacjenta";
        }
        else
        {
            var therapists = await _userManager.Users
                .Where(u => u.IsTherapist)
                .ToListAsync();

            ViewBag.TargetUsers = new SelectList(
    therapists.Select(u => new
    {
        u.Id,
        FullName = u.FirstName + " " + u.LastName
    }),
    "Id",
    "FullName"
);
            ViewBag.RoleLabel = "Wybierz Terapeutę";
        }

        return View(new CreateConsultationDto());
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Create(CreateConsultationDto dto)
    {
        var user = await _userManager.GetUserAsync(User);
        if (user == null) return NotFound();

        if (dto.StartTime >= dto.EndTime)
            ModelState.AddModelError("", "Czas zakończenia musi być po rozpoczęciu.");

        if (!ModelState.IsValid)
        {
            return await Create(); // prościej i bez duplikacji
        }

        var consultation = new Consultation
        {
            Id = Guid.NewGuid(),
            StartTime = dto.StartTime,
            EndTime = dto.EndTime,
            Notes = dto.Notes,
            StatusId = 1
        };

        if (user.IsTherapist)
        {
            consultation.TherapistId = user.Id;
            consultation.PatientId = dto.TargetUserId;
        }
        else
        {
            consultation.PatientId = user.Id;
            consultation.TherapistId = dto.TargetUserId;
        }

        await _consultationService.CreateConsultationAsync(consultation);
        return RedirectToAction(nameof(Index));
    }
}