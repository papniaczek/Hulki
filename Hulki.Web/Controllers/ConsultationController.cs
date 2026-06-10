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
    private readonly INotificationService _notificationService;
    private readonly IPdfReportService _pdfReportService;

    public ConsultationController(
        IConsultationService consultationService,
        UserManager<AppUser> userManager,
        ApplicationDbContext context,
        INotificationService notificationService,
        IPdfReportService pdfReportService)
    {
        _consultationService = consultationService;
        _userManager = userManager;
        _context = context;
        _notificationService = notificationService;
        _pdfReportService = pdfReportService;
    }

    public async Task<IActionResult> Index()
    {
        var user = await _userManager.GetUserAsync(User);
        if (user == null) return NotFound();

        var consultations = await _consultationService.GetUserConsultationsAsync(user.Id);
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
                patients.Select(u => new { u.Id, FullName = u.FirstName + " " + u.LastName }),
                "Id", "FullName");
            ViewBag.RoleLabel = "Wybierz Pacjenta";
        }
        else
        {


            var therapists = await _userManager.Users
                .Where(u => u.IsTherapist)
                .ToListAsync();

            ViewBag.TargetUsers = new SelectList(
                therapists.Select(u => new { u.Id, FullName = u.FirstName + " " + u.LastName }),
                "Id", "FullName");
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
            return await Create();

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


        string recipientId = user.IsTherapist ? consultation.PatientId : consultation.TherapistId;
        string senderRole = user.IsTherapist ? "Terapeuta" : "Pacjent";
        await _notificationService.SendNotificationAsync(
            recipientId,
            $"{senderRole} {user.FirstName} {user.LastName} zaplanował(a) z Tobą wizytę na {consultation.StartTime:dd.MM.yyyy HH:mm}."
        );

        TempData["SuccessMessage"] = "Wizyta została zaplanowana.";
        return RedirectToAction(nameof(Index));
    }

    [HttpGet]
    public async Task<IActionResult> Details(Guid id)
    {
        var user = await _userManager.GetUserAsync(User);
        if (user == null) return NotFound();

        var consultation = await _context.Consultations
            .Include(c => c.Patient)
            .Include(c => c.Therapist)
            .Include(c => c.Details)
            .FirstOrDefaultAsync(c => c.Id == id);

        if (consultation == null) return NotFound("Nie znaleziono takiej konsultacji.");

        if (consultation.PatientId != user.Id && consultation.TherapistId != user.Id)
            return Forbid();

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

        ViewBag.IsTherapist = user.IsTherapist;
        return View(consultation);
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> SaveDetails(Guid consultationId, VisitDetails detailsFromForm)
    {
        var user = await _userManager.GetUserAsync(User);
        if (user == null || !user.IsTherapist) return Forbid();

        var consultation = await _consultationService.GetConsultationByIdAsync(consultationId);
        if (consultation == null) return NotFound();

        if (consultation.Details == null)
            consultation.Details = new VisitDetails { ConsultationId = consultationId };

        consultation.Details.MedicalHistory = detailsFromForm.MedicalHistory;
        consultation.Details.Diagnosis = detailsFromForm.Diagnosis;
        consultation.Details.Recommendations = detailsFromForm.Recommendations;
        consultation.Details.InternalNotes = detailsFromForm.InternalNotes;

        await _consultationService.UpdateConsultationAsync(consultation);


        await _notificationService.SendNotificationAsync(
            consultation.PatientId,
            $"Terapeuta {user.FirstName} {user.LastName} zaktualizował(a) szczegóły Twojej wizyty z dnia {consultation.StartTime:dd.MM.yyyy}."
        );

        TempData["SuccessMessage"] = "Karta wizyty została zapisana.";
        return RedirectToAction(nameof(Details), new { id = consultationId });
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> ChangeStatus(Guid id, int statusId)
    {
        var user = await _userManager.GetUserAsync(User);
        if (user == null) return NotFound();

        if (!user.IsTherapist)
        {
            TempData["ErrorMessage"] = "Brak uprawnień do zmiany statusu wizyty.";
            return RedirectToAction(nameof(Index));
        }

        var consultation = await _context.Consultations
            .FirstOrDefaultAsync(c => c.Id == id);

        if (consultation == null)
        {
            TempData["ErrorMessage"] = "Nie znaleziono wizyty.";
            return RedirectToAction(nameof(Index));
        }

        if (consultation.TherapistId != user.Id)
            return Forbid();

        if (consultation.StatusId != 1)
        {
            TempData["ErrorMessage"] = "Można zmieniać status tylko zaplanowanych wizyt.";
            return RedirectToAction(nameof(Index));
        }

        if (statusId != 2 && statusId != 3)
        {
            TempData["ErrorMessage"] = "Nieprawidłowy status.";
            return RedirectToAction(nameof(Index));
        }

        consultation.StatusId = statusId;
        await _context.SaveChangesAsync();


        string statusMessage = statusId == 2
            ? "zakończona"
            : "odwołana";
        await _notificationService.SendNotificationAsync(
            consultation.PatientId,
            $"Status Twojej wizyty z dnia {consultation.StartTime:dd.MM.yyyy} został zmieniony na: {statusMessage}."
        );

        TempData["SuccessMessage"] = statusId == 2
            ? "Wizyta została oznaczona jako zakończona."
            : "Wizyta została odwołana.";

        return RedirectToAction(nameof(Index));
    }

    [HttpGet]
    public async Task<IActionResult> DownloadPdf(Guid id)
    {
        var user = await _userManager.GetUserAsync(User);
        if (user == null) return Unauthorized();

        var consultation = await _context.Consultations
            .FirstOrDefaultAsync(c => c.Id == id);

        if (consultation == null)
        {
            TempData["ErrorMessage"] = "Nie znaleziono konsultacji.";
            return RedirectToAction(nameof(Index));
        }

        if (consultation.PatientId != user.Id && consultation.TherapistId != user.Id)
            return Forbid();

        try
        {
            var pdfBytes = await _pdfReportService.GenerateConsultationReportAsync(id);

            var fileName = $"Konsultacja_{consultation.StartTime:yyyyMMdd}.pdf";
            return File(pdfBytes, "application/pdf", fileName);
        }
        catch (Exception ex)
        {
            TempData["ErrorMessage"] = $"Błąd podczas generowania PDF: {ex.Message}";
            return RedirectToAction(nameof(Details), new { id });
        }
    }
}
