using Hulki.Web.Data;
using Hulki.Web.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Linq;
using System.Threading.Tasks;

namespace Hulki.Web.Controllers;

[Authorize] // Na ten moment tylko dla zalogowanych
public class TherapyGroupController : Controller
{
    private readonly ApplicationDbContext _context;

    public TherapyGroupController(ApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<IActionResult> Index()
    {
        var groups = await _context.TherapyGroups.Include(t => t.TherapyType).ToListAsync();
        return View(groups);
    }

    [HttpGet]
    public IActionResult Create()
    {
        ViewBag.TherapyTypes = _context.TherapyTypes.ToList();
        return View();
    }

    [HttpPost]
    public async Task<IActionResult> Create(TherapyGroup group)
    {
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

    [HttpGet]
    public async Task<IActionResult> Edit(int id)
    {
        var group = await _context.TherapyGroups.FindAsync(id);
        if (group == null) return NotFound();

        ViewBag.TherapyTypes = _context.TherapyTypes.ToList();
        return View(group);
    }

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