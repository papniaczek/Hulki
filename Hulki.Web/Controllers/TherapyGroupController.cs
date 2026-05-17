using Hulki.Web.Data;
using Hulki.Web.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Linq;
using System.Threading.Tasks;

namespace Hulki.Web.Controllers;

[Authorize] 
public class TherapyGroupController : Controller
{
    private readonly ApplicationDbContext _context;
    private readonly UserManager<AppUser> _userManager;

    public TherapyGroupController(ApplicationDbContext context, UserManager<AppUser> userManager)
    {
        _context = context;
        _userManager = userManager;
    }

    // 1. LISTA GRUP
    public async Task<IActionResult> Index()
    {
        // Wywołujemy siewnik typów terapii również tutaj na wypadek, gdyby ktoś najpierw wszedł na listę
        await SeedTherapyTypesIfNotExists();

        var user = await _userManager.GetUserAsync(User);

        var groups = await _context.TherapyGroups
            .Include(t => t.TherapyType)
            .Include(t => t.PatientGroups)
            .ToListAsync();

        ViewBag.CurrentUserId = user?.Id;

        return View(groups);
    }

    // 2. DOŁĄCZANIE DO GRUPY
    [HttpPost]
    public async Task<IActionResult> Join(int id)
    {
        var user = await _userManager.GetUserAsync(User);
        if (user == null) return Challenge();

        var group = await _context.TherapyGroups
            .Include(g => g.PatientGroups)
            .FirstOrDefaultAsync(g => g.Id == id);

        if (group == null) return NotFound();

        if (group.PatientGroups.Any(pg => pg.AppUserId == user.Id))
        {
            TempData["ErrorMessage"] = "Złożyłeś już wniosek do tej grupy lub jesteś jej członkiem.";
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
            IsApproved = false 
        };

        _context.PatientGroups.Add(application);
        await _context.SaveChangesAsync();

        TempData["SuccessMessage"] = "Wniosek o dołączenie został wysłany! Poczekaj na zatwierdzenie przez terapeutę.";
        return RedirectToAction(nameof(Index));
    }

    // 3. ZARZĄDZANIE GRUPĄ I WNIOSKAMI
    [Authorize(Roles = "Terapeuta, Admin")]
    public async Task<IActionResult> Manage(int id)
    {
        var group = await _context.TherapyGroups
            .Include(g => g.PatientGroups)
                .ThenInclude(pg => pg.AppUser) 
            .FirstOrDefaultAsync(g => g.Id == id);

        if (group == null) return NotFound();

        return View(group);
    }
    
    // --- DASHBOARD GRUPY (Dla członków i personelu) ---
    [HttpGet]
    public async Task<IActionResult> Details(int id)
    {
        var user = await _userManager.GetUserAsync(User);
        if (user == null) return Challenge();

        // Sprawdzamy, czy użytkownik ma prawo tu być
        bool isStaff = User.IsInRole("Admin") || User.IsInRole("Terapeuta");
        
        var group = await _context.TherapyGroups
            .Include(g => g.PatientGroups)
            .Include(g => g.TherapyType)
            .FirstOrDefaultAsync(g => g.Id == id);

        if (group == null) return NotFound();

        bool isMember = group.PatientGroups.Any(pg => pg.AppUserId == user.Id && pg.IsApproved);

        if (!isStaff && !isMember)
        {
            TempData["ErrorMessage"] = "Nie masz dostępu do tej grupy. Musisz być jej zatwierdzonym członkiem.";
            return RedirectToAction(nameof(Index));
        }

        // Pobieramy wiadomości z tablicy
        var messages = await _context.GroupMessages
            .Include(m => m.AppUser)
            .Where(m => m.TherapyGroupId == id)
            .OrderByDescending(m => m.CreatedAt) // Najnowsze na górze
            .ToListAsync();

        ViewBag.Messages = messages;
        ViewBag.CurrentUserId = user.Id;
        ViewBag.IsStaff = isStaff;

        return View(group);
    }

    // --- DODAWANIE WPISU NA TABLICĘ ---
    [HttpPost]
    public async Task<IActionResult> AddMessage(int groupId, string content)
    {
        var user = await _userManager.GetUserAsync(User);
        if (user == null || string.IsNullOrWhiteSpace(content)) return RedirectToAction(nameof(Details), new { id = groupId });

        // Dodatkowe zabezpieczenie można by tu dodać, ale polegamy na tym z Details
        var message = new GroupMessage
        {
            TherapyGroupId = groupId,
            AppUserId = user.Id,
            Content = content,
            CreatedAt = DateTime.Now
        };

        _context.GroupMessages.Add(message);
        await _context.SaveChangesAsync();

        return RedirectToAction(nameof(Details), new { id = groupId });
    }

    // 4. ZATWIERDZANIE WNIOSKU
    [HttpPost]
    [Authorize(Roles = "Terapeuta, Admin")]
    public async Task<IActionResult> Approve(int groupId, string userId)
    {
        var membership = await _context.PatientGroups
            .FirstOrDefaultAsync(pg => pg.TherapyGroupId == groupId && pg.AppUserId == userId);

        if (membership != null)
        {
            membership.IsApproved = true;
            _context.PatientGroups.Update(membership);
            await _context.SaveChangesAsync();
            TempData["SuccessMessage"] = "Pacjent został przyjęty do grupy!";
        }

        return RedirectToAction(nameof(Manage), new { id = groupId });
    }

    // 5. ODRZUCANIE/USUNIĘCIE Z GRUPY
    [HttpPost]
    [Authorize(Roles = "Terapeuta, Admin")]
    public async Task<IActionResult> Reject(int groupId, string userId)
    {
        var membership = await _context.PatientGroups
            .FirstOrDefaultAsync(pg => pg.TherapyGroupId == groupId && pg.AppUserId == userId);

        if (membership != null)
        {
            _context.PatientGroups.Remove(membership);
            await _context.SaveChangesAsync();
            TempData["SuccessMessage"] = "Wniosek został odrzucony / Pacjent został usunięty.";
        }

        return RedirectToAction(nameof(Manage), new { id = groupId });
    }

    // --- STANDARDOWE CRUD DLA TERAPEUTY ---

    [HttpGet]
    [Authorize(Roles = "Terapeuta, Admin")]
    public async Task<IActionResult> Create()
    {
        // Automatyczne uzupełnienie słownika przed załadowaniem widoku formularza
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
        if (group == null) return NotFound();

        ViewBag.TherapyTypes = await _context.TherapyTypes.ToListAsync();
        return View(group);
    }

    [HttpPost]
    [Authorize(Roles = "Terapeuta, Admin")]
    public async Task<IActionResult> Edit(int id, TherapyGroup group)
    {
        if (id != group.Id) return NotFound();

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

    // --- AUTOMATYCZNY SIEWNIK DANYCH SŁOWNIKOWYCH ---
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