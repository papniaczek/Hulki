using Hulki.Web.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using Hulki.Web.Models;
using Hulki.Web.Models.Dto;
using Hulki.Web.Data;

namespace Hulki.Web.Controllers;

[Authorize]
public class ConsultationController : Controller
{
    private readonly IConsultationService _consultationService;
    private readonly UserManager<AppUser> _userManager;
    private readonly ApplicationDbContext _context;
    public ConsultationController(
    IConsultationService consultationService,
    UserManager<AppUser> userManager,
    ApplicationDbContext context)
    {
        _consultationService = consultationService;
        _userManager = userManager;
        _context = context;
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
    [HttpGet]
    public async Task<IActionResult> Details(Guid id)
    {
        var user = await _userManager.GetUserAsync(User);
        
        if (user == null) return NotFound();

        // Pobieramy konsultację z serwisu (musi zawierać dołączone Details)
        var consultation = await _context.Consultations
    .Include(c => c.Patient)
    .Include(c => c.Therapist)
    .Include(c => c.Details)
    .FirstOrDefaultAsync(c => c.Id == id);
        if (consultation == null) return NotFound("Nie znaleziono takiej konsultacji.");

        // Zabezpieczenie: Tylko przypisany pacjent lub terapeuta może zobaczyć szczegóły
        if (consultation.PatientId != user.Id && consultation.TherapistId != user.Id)
        {
            return Forbid();
        }

        // Jeśli konsultacja nie ma jeszcze utworzonego obiektu szczegółów, 
        // tworzymy pusty obiekt, żeby widok nie wywalił NullReferenceException
        if (consultation.Details == null)
        {
            consultation.Details = new VisitDetails
            {
                ConsultationId = id,
                MedicalHistory = string.Empty,
                Diagnosis = string.Empty,
                Recommendations = string.Empty,
                InternalNotes = string.Empty
            };
        }

        // Przekazujemy również rolę użytkownika, by w widoku ukryć "InternalNotes" (notatki lekarskie) przed pacjentem
        ViewBag.IsTherapist = user.IsTherapist;

        return View(consultation);
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    // Usunięto wadliwy atrybut [Authorize(Roles = "Therapist")]
    public async Task<IActionResult> SaveDetails(Guid consultationId, VisitDetails detailsFromForm)
    {
        var user = await _userManager.GetUserAsync(User);
        // Ta linijka w zupełności wystarczy do zabezpieczenia akcji:
        if (user == null || !user.IsTherapist) return Forbid();

        var consultation = await _consultationService.GetConsultationByIdAsync(consultationId);
        if (consultation == null) return NotFound();

        if (consultation.Details == null)
        {
            consultation.Details = new VisitDetails { ConsultationId = consultationId };
        }

        consultation.Details.MedicalHistory = detailsFromForm.MedicalHistory;
        consultation.Details.Diagnosis = detailsFromForm.Diagnosis;
        consultation.Details.Recommendations = detailsFromForm.Recommendations;
        consultation.Details.InternalNotes = detailsFromForm.InternalNotes;

        await _consultationService.UpdateConsultationAsync(consultation);

        return RedirectToAction(nameof(Details), new { id = consultationId });
    }

}

