using Hulki.Web.Data;
using Hulki.Web.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Linq;
using System.Threading.Tasks;

namespace Hulki.Web.Controllers;

[Authorize] // Na razie blokujemy dostęp tylko dla zalogowanych (w przyszłości można to ograniczyć do roli "Terapeuta")
public class TherapyGroupController : Controller
{
    private readonly ApplicationDbContext _context;

    public TherapyGroupController(ApplicationDbContext context)
    {
        _context = context;
    }

    // 1. READ: Lista wszystkich grup
    public async Task<IActionResult> Index()
    {
        // Pobieramy grupy i dołączamy typ terapii (relacja)
        var groups = await _context.TherapyGroups.Include(t => t.TherapyType).ToListAsync();
        return View(groups);
    }

    // 2. CREATE: Wyświetlenie formularza dodawania
    [HttpGet]
    public IActionResult Create()
    {
        // Pobieramy typy terapii, żeby wrzucić je do <select> (listy rozwijanej)
        ViewBag.TherapyTypes = _context.TherapyTypes.ToList();
        return View();
    }

    // 2. CREATE: Przetworzenie danych z formularza
    [HttpPost]
    public async Task<IActionResult> Create(TherapyGroup group)
    {
        // Usuwamy z walidacji właściwość nawigacyjną, bo EF Core by krzyczał
        ModelState.Remove("TherapyType"); 

        if (ModelState.IsValid)
        {
            _context.TherapyGroups.Add(group);
            await _context.SaveChangesAsync();
            TempData["SuccessMessage"] = "Pomyślnie dodano nową grupę wsparcia!";
            return RedirectToAction(nameof(Index));
        }

        ViewBag.TherapyTypes = _context.TherapyTypes.ToList();
        return View(group);
    }

    // 3. UPDATE: Wyświetlenie formularza edycji
    [HttpGet]
    public async Task<IActionResult> Edit(int id)
    {
        var group = await _context.TherapyGroups.FindAsync(id);
        if (group == null) return NotFound();

        ViewBag.TherapyTypes = _context.TherapyTypes.ToList();
        return View(group);
    }

    // 3. UPDATE: Przetworzenie edycji
    [HttpPost]
    public async Task<IActionResult> Edit(int id, TherapyGroup group)
    {
        if (id != group.Id) return NotFound();

        ModelState.Remove("TherapyType");

        if (ModelState.IsValid)
        {
            _context.TherapyGroups.Update(group);
            await _context.SaveChangesAsync();
            TempData["SuccessMessage"] = "Pomyślnie zaktualizowano dane grupy!";
            return RedirectToAction(nameof(Index));
        }

        ViewBag.TherapyTypes = _context.TherapyTypes.ToList();
        return View(group);
    }

    // 4. DELETE: Usunięcie grupy
    [HttpPost]
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
}