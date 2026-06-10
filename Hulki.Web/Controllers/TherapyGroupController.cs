using System.IO;
using System.Linq;
using System.Threading.Tasks;
using Hulki.Web.Data;
using Hulki.Web.Models;
using Hulki.Web.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace Hulki.Web.Controllers;

[Authorize]
public class TherapyGroupController : Controller
{
    private readonly ApplicationDbContext _context;
    private readonly UserManager<AppUser> _userManager;
    private readonly INotificationService _notificationService;

    public TherapyGroupController(
        ApplicationDbContext context,
        UserManager<AppUser> userManager,
        INotificationService notificationService
    )
    {
        _context = context;
        _userManager = userManager;
        _notificationService = notificationService;
    }

    public async Task<IActionResult> Index()
    {
        await SeedTherapyTypesIfNotExists();

        var user = await _userManager.GetUserAsync(User);

        var groups = await _context
            .TherapyGroups.Include(t => t.TherapyType)
            .Include(t => t.PatientGroups)
            .ToListAsync();

        ViewBag.CurrentUserId = user?.Id;

        return View(groups);
    }

    [HttpPost]
    public async Task<IActionResult> Join(int id)
    {
        var user = await _userManager.GetUserAsync(User);
        if (user == null)
            return Challenge();

        var group = await _context
            .TherapyGroups.Include(g => g.PatientGroups)
            .FirstOrDefaultAsync(g => g.Id == id);

        if (group == null)
            return NotFound();

        if (group.PatientGroups.Any(pg => pg.AppUserId == user.Id))
        {
            TempData["ErrorMessage"] =
                "Złożyłeś już wniosek do tej grupy lub jesteś jej członkiem.";
            return RedirectToAction(nameof(Index));
        }

        int approvedCount = group.PatientGroups.Count(pg => pg.IsApproved);
        if (approvedCount >= group.MaxParticipants)
        {
            TempData["ErrorMessage"] = "Brak wolnych miejsc w tej grupie.";
            return RedirectToAction(nameof(Index));
        }

        var application = new PatientGroup
        {
            AppUserId = user.Id,
            TherapyGroupId = group.Id,
            JoinedDate = System.DateTime.Now,
            IsApproved = false,
        };

        _context.PatientGroups.Add(application);
        await _context.SaveChangesAsync();

        TempData["SuccessMessage"] =
            "Wniosek o dołączenie został wysłany! Poczekaj na zatwierdzenie przez terapeutę.";
        return RedirectToAction(nameof(Index));
    }

    [Authorize(Roles = "Terapeuta, Admin")]
    public async Task<IActionResult> Manage(int id)
    {
        var group = await _context
            .TherapyGroups.Include(g => g.PatientGroups)
                .ThenInclude(pg => pg.AppUser)
            .FirstOrDefaultAsync(g => g.Id == id);

        if (group == null)
            return NotFound();

        return View(group);
    }

    [HttpGet]
    public async Task<IActionResult> Details(int id)
    {
        var user = await _userManager.GetUserAsync(User);
        if (user == null)
            return Challenge();

        bool isStaff = User.IsInRole("Admin") || User.IsInRole("Terapeuta");

        var group = await _context
            .TherapyGroups.Include(g => g.PatientGroups)
            .Include(g => g.TherapyType)
            .FirstOrDefaultAsync(g => g.Id == id);

        if (group == null)
            return NotFound();

        bool isMember = group.PatientGroups.Any(pg => pg.AppUserId == user.Id && pg.IsApproved);

        if (!isStaff && !isMember)
        {
            TempData["ErrorMessage"] =
                "Nie masz dostępu do tej grupy. Musisz być jej zatwierdzonym członkiem.";
            return RedirectToAction(nameof(Index));
        }

        var messages = await _context
            .GroupMessages.Include(m => m.AppUser)
            .Where(m => m.TherapyGroupId == id)
            .OrderByDescending(m => m.CreatedAt)
            .ToListAsync();

        var quests = await _context
            .GroupQuests.Include(q => q.Submissions)
                .ThenInclude(s => s.AppUser)
            .Where(q => q.TherapyGroupId == id)
            .OrderByDescending(q => q.CreatedAt)
            .ToListAsync();

        var resources = await _context
            .GroupResources.Where(r => r.TherapyGroupId == id)
            .OrderByDescending(r => r.CreatedAt)
            .ToListAsync();

        ViewBag.Messages = messages;
        ViewBag.Quests = quests;
        ViewBag.Resources = resources;
        ViewBag.CurrentUserId = user.Id;
        ViewBag.IsStaff = isStaff;

        return View(group);
    }

    [HttpPost]
    public async Task<IActionResult> AddMessage(int groupId, string content)
    {
        var user = await _userManager.GetUserAsync(User);
        if (user == null || string.IsNullOrWhiteSpace(content))
            return RedirectToAction(nameof(Details), new { id = groupId });

        var message = new GroupMessage
        {
            TherapyGroupId = groupId,
            AppUserId = user.Id,
            Content = content,
            CreatedAt = DateTime.Now,
        };

        _context.GroupMessages.Add(message);
        await _context.SaveChangesAsync();

        return RedirectToAction(nameof(Details), new { id = groupId });
    }

    [HttpPost]
    [Authorize(Roles = "Terapeuta, Admin")]
    public async Task<IActionResult> Approve(int groupId, string userId)
    {
        var membership = await _context.PatientGroups.FirstOrDefaultAsync(pg =>
            pg.TherapyGroupId == groupId && pg.AppUserId == userId
        );

        if (membership != null)
        {
            membership.IsApproved = true;
            _context.PatientGroups.Update(membership);
            await _context.SaveChangesAsync();

            await _notificationService.SendNotificationAsync(
                userId,
                "Twój wniosek o dołączenie do grupy terapeutycznej został zaakceptowany! Zobacz zakładkę grupy."
            );

            TempData["SuccessMessage"] = "Pacjent został przyjęty do grupy!";
        }

        return RedirectToAction(nameof(Manage), new { id = groupId });
    }

    [HttpPost]
    [Authorize(Roles = "Terapeuta, Admin")]
    public async Task<IActionResult> Reject(int groupId, string userId)
    {
        var membership = await _context.PatientGroups.FirstOrDefaultAsync(pg =>
            pg.TherapyGroupId == groupId && pg.AppUserId == userId
        );

        if (membership != null)
        {
            _context.PatientGroups.Remove(membership);
            await _context.SaveChangesAsync();

            await _notificationService.SendNotificationAsync(
                userId,
                "Twój wniosek o dołączenie do grupy został odrzucony przez terapeutę."
            );

            TempData["ErrorMessage"] = "Wniosek został odrzucony. Pacjent został usunięty.";
        }

        return RedirectToAction(nameof(Manage), new { id = groupId });
    }

    [HttpGet]
    [Authorize(Roles = "Terapeuta, Admin")]
    public async Task<IActionResult> Create()
    {
        await SeedTherapyTypesIfNotExists();

        ViewBag.TherapyTypes = await _context.TherapyTypes.ToListAsync();
        return View();
    }

    [HttpPost]
    [Authorize(Roles = "Terapeuta, Admin")]
    public async Task<IActionResult> Create(TherapyGroup group)
    {
        ModelState.Remove("TherapyType");
        ModelState.Remove("PatientGroups");

        if (ModelState.IsValid)
        {
            _context.TherapyGroups.Add(group);
            await _context.SaveChangesAsync();
            TempData["SuccessMessage"] = "Pomyślnie dodano nową grupę wsparcia!";
            return RedirectToAction(nameof(Index));
        }

        ViewBag.TherapyTypes = await _context.TherapyTypes.ToListAsync();
        return View(group);
    }

    [HttpGet]
    [Authorize(Roles = "Terapeuta, Admin")]
    public async Task<IActionResult> Edit(int id)
    {
        var group = await _context.TherapyGroups.FindAsync(id);
        if (group == null)
            return NotFound();

        ViewBag.TherapyTypes = await _context.TherapyTypes.ToListAsync();
        return View(group);
    }

    [HttpPost]
    [Authorize(Roles = "Terapeuta, Admin")]
    public async Task<IActionResult> Edit(int id, TherapyGroup group)
    {
        if (id != group.Id)
            return NotFound();

        ModelState.Remove("TherapyType");
        ModelState.Remove("PatientGroups");

        if (ModelState.IsValid)
        {
            _context.TherapyGroups.Update(group);
            await _context.SaveChangesAsync();
            TempData["SuccessMessage"] = "Pomyślnie zaktualizowano dane grupy!";
            return RedirectToAction(nameof(Index));
        }

        ViewBag.TherapyTypes = await _context.TherapyTypes.ToListAsync();
        return View(group);
    }

    [HttpPost]
    [Authorize(Roles = "Terapeuta, Admin")]
    public async Task<IActionResult> Delete(int id)
    {
        var group = await _context.TherapyGroups.FindAsync(id);
        if (group != null)
        {
            _context.TherapyGroups.Remove(group);
            await _context.SaveChangesAsync();
            TempData["SuccessMessage"] = "Grupa została pomyślnie usunięta.";
        }
        return RedirectToAction(nameof(Index));
    }

    [HttpPost]
    [Authorize(Roles = "Terapeuta, Admin")]
    public async Task<IActionResult> CreateQuest(
        int groupId,
        string title,
        string description,
        string questType,
        int rewardPoints,
        string? optionA,
        string? optionB,
        string? optionC,
        string? optionD,
        string? correctOption
    )
    {
        if (!string.IsNullOrWhiteSpace(title) && !string.IsNullOrWhiteSpace(description))
        {
            var quest = new GroupQuest
            {
                TherapyGroupId = groupId,
                Title = title,
                Description = description,
                QuestType = questType,
                RewardPoints = rewardPoints,

                OptionA = questType == "QuizABCD" ? optionA : null,
                OptionB = questType == "QuizABCD" ? optionB : null,
                OptionC = questType == "QuizABCD" ? optionC : null,
                OptionD = questType == "QuizABCD" ? optionD : null,
                CorrectOption = questType == "QuizABCD" ? correctOption : null,
            };
            _context.GroupQuests.Add(quest);
            await _context.SaveChangesAsync();
            TempData["SuccessMessage"] = "Nowe wyzwanie zostało dodane!";
        }
        return RedirectToAction(nameof(Details), new { id = groupId });
    }

    [HttpPost]
    public async Task<IActionResult> SubmitQuest(int questId, int groupId, string answerText)
    {
        var user = await _userManager.GetUserAsync(User);
        if (user == null || string.IsNullOrWhiteSpace(answerText))
            return RedirectToAction(nameof(Details), new { id = groupId });

        var quest = await _context.GroupQuests.FindAsync(questId);
        if (quest == null)
            return NotFound();

        var submission = new QuestSubmission
        {
            GroupQuestId = questId,
            AppUserId = user.Id,
            AnswerText = answerText,
        };

        if (quest.QuestType == "QuizABCD")
        {
            submission.IsEvaluated = true;
            submission.IsAccepted = (answerText == quest.CorrectOption);

            if (submission.IsAccepted)
            {
                var wallet = await _context.Wallets.FirstOrDefaultAsync(w =>
                    w.AppUserId == user.Id
                );
                if (wallet != null)
                {
                    wallet.Balance += quest.RewardPoints;
                    _context.PointTransactions.Add(
                        new PointTransaction
                        {
                            Id = Guid.NewGuid(),
                            WalletId = wallet.Id,
                            Amount = quest.RewardPoints,
                            Description = $"Prawidłowa odpowiedź w quizie: {quest.Title}",
                            TransactionDate = DateTime.Now,
                        }
                    );
                }
            }
        }

        _context.QuestSubmissions.Add(submission);
        await _context.SaveChangesAsync();

        TempData["SuccessMessage"] =
            quest.QuestType == "QuizABCD"
                ? "Quiz został sprawdzony automatycznie!"
                : "Odpowiedź wysłana! Oczekuje na weryfikację terapeuty.";

        return RedirectToAction(nameof(Details), new { id = groupId });
    }

    [HttpPost]
    [Authorize(Roles = "Terapeuta, Admin")]
    public async Task<IActionResult> EvaluateQuest(int submissionId, int groupId, bool accept)
    {
        var submission = await _context
            .QuestSubmissions.Include(s => s.GroupQuest)
            .FirstOrDefaultAsync(s => s.Id == submissionId);

        if (submission != null && !submission.IsEvaluated)
        {
            submission.IsEvaluated = true;
            submission.IsAccepted = accept;

            if (accept)
            {
                var wallet = await _context.Wallets.FirstOrDefaultAsync(w =>
                    w.AppUserId == submission.AppUserId
                );
                if (wallet != null)
                {
                    wallet.Balance += submission.GroupQuest.RewardPoints;

                    _context.PointTransactions.Add(
                        new PointTransaction
                        {
                            Id = Guid.NewGuid(),
                            WalletId = wallet.Id,
                            Amount = submission.GroupQuest.RewardPoints,
                            Description =
                                $"Nagroda za grupowe wyzwanie: {submission.GroupQuest.Title}",
                            TransactionDate = DateTime.Now,
                        }
                    );
                }
            }

            _context.QuestSubmissions.Update(submission);
            await _context.SaveChangesAsync();
        }

        return RedirectToAction(nameof(Details), new { id = groupId });
    }

    [HttpPost]
    [Authorize(Roles = "Terapeuta, Admin")]
    public async Task<IActionResult> DeleteQuest(int questId, int groupId)
    {
        var quest = await _context
            .GroupQuests.Include(q => q.Submissions)
            .FirstOrDefaultAsync(q => q.Id == questId);

        if (quest != null)
        {
            _context.QuestSubmissions.RemoveRange(quest.Submissions);
            _context.GroupQuests.Remove(quest);
            await _context.SaveChangesAsync();
            TempData["SuccessMessage"] = "Wyzwanie zostało bezpowrotnie usunięte.";
        }

        return RedirectToAction(nameof(Details), new { id = groupId });
    }

    [HttpPost]
    [Authorize(Roles = "Terapeuta, Admin")]
    public async Task<IActionResult> UploadResource(
        int groupId,
        string title,
        IFormFile uploadedFile
    )
    {
        if (string.IsNullOrWhiteSpace(title) || uploadedFile == null || uploadedFile.Length == 0)
        {
            TempData["ErrorMessage"] = "Podaj prawidłowy tytuł i wybierz plik.";
            return RedirectToAction(nameof(Details), new { id = groupId });
        }

        var allowedExtensions = new[] { ".jpg", ".jpeg", ".png", ".pdf" };
        var fileExtension = Path.GetExtension(uploadedFile.FileName).ToLowerInvariant();
        if (!allowedExtensions.Contains(fileExtension))
        {
            TempData["ErrorMessage"] =
                "Nieobsługiwany format pliku. Obsługiwane rozszerzenia to: .jpg, .jpeg, .png, .pdf";
            return RedirectToAction(nameof(Details), new { id = groupId });
        }

        var uploadsFolder = Path.Combine(
            Directory.GetCurrentDirectory(),
            "wwwroot",
            "uploads",
            "resources"
        );
        if (!Directory.Exists(uploadsFolder))
        {
            Directory.CreateDirectory(uploadsFolder);
        }

        var uniqueFileName =
            Guid.NewGuid().ToString() + "_" + Path.GetFileName(uploadedFile.FileName);
        var filePath = Path.Combine(uploadsFolder, uniqueFileName);

        using (var fileStream = new FileStream(filePath, FileMode.Create))
        {
            await uploadedFile.CopyToAsync(fileStream);
        }

        var resource = new GroupResource
        {
            TherapyGroupId = groupId,
            Title = title,
            FilePath = "/uploads/resources/" + uniqueFileName,
        };

        _context.GroupResources.Add(resource);
        await _context.SaveChangesAsync();

        TempData["SuccessMessage"] = $"Pomyślnie udostępniono materiał: {title}!";
        return RedirectToAction(nameof(Details), new { id = groupId });
    }

    private async Task SeedTherapyTypesIfNotExists()
    {
        if (!await _context.TherapyTypes.AnyAsync())
        {
            _context.TherapyTypes.AddRange(
                new TherapyType { Name = "Uzależnień i Współuzależnień" },
                new TherapyType { Name = "Indywidualna Kognitywna" },
                new TherapyType { Name = "Grupowa Grupa Wsparcia" },
                new TherapyType { Name = "Behawioralno-Motywacyjna" },
                new TherapyType { Name = "Zapobiegania Nawrotom" }
            );
            await _context.SaveChangesAsync();
        }
    }
}
