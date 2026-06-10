using Hulki.Web.Data;
using Hulki.Web.Models;
using Hulki.Web.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System;
using System.Linq;
using System.Threading.Tasks;

namespace Hulki.Web.Controllers;

[Authorize]
public class ForumController : Controller
{
    private readonly ApplicationDbContext _context;
    private readonly UserManager<AppUser> _userManager;
    private readonly INotificationService _notificationService;

    public ForumController(
        ApplicationDbContext context,
        UserManager<AppUser> userManager,
        INotificationService notificationService)
    {
        _context = context;
        _userManager = userManager;
        _notificationService = notificationService;
    }


    public async Task<IActionResult> Index()
    {
        await SeedForumCategoriesIfNotExists();

        var categories = await _context.ForumCategories
            .Include(c => c.ForumTopics)
            .ToListAsync();

        return View(categories);
    }


    public async Task<IActionResult> Category(int id)
    {
        var category = await _context.ForumCategories
            .Include(c => c.ForumTopics)
                .ThenInclude(t => t.AppUser)
            .Include(c => c.ForumTopics)
                .ThenInclude(t => t.Posts)
            .FirstOrDefaultAsync(c => c.Id == id);

        if (category == null) return NotFound();

        return View(category);
    }


    public async Task<IActionResult> Topic(Guid id)
    {
        var topic = await _context.ForumTopics
            .Include(t => t.AppUser)
            .Include(t => t.ForumCategory)
            .Include(t => t.Posts)
                .ThenInclude(p => p.AppUser)
            .FirstOrDefaultAsync(t => t.Id == id);

        if (topic == null) return NotFound();

        return View(topic);
    }


    [HttpGet]
    public async Task<IActionResult> CreateTopic(int categoryId)
    {
        var category = await _context.ForumCategories.FindAsync(categoryId);
        if (category == null) return NotFound();

        ViewBag.Category = category;
        return View();
    }


    [HttpPost]
    public async Task<IActionResult> CreateTopic(int forumCategoryId, string title, string content)
    {
        var user = await _userManager.GetUserAsync(User);

        if (string.IsNullOrWhiteSpace(title) || string.IsNullOrWhiteSpace(content))
        {
            TempData["ErrorMessage"] = "Tytuł i treść tematu nie mogą być puste!";
            return RedirectToAction(nameof(CreateTopic), new { categoryId = forumCategoryId });
        }

        var newTopic = new ForumTopic
        {
            Id = Guid.NewGuid(),
            Title = title,
            Content = content,
            CreatedAt = DateTime.Now,
            ForumCategoryId = forumCategoryId,
            AppUserId = user.Id
        };

        _context.ForumTopics.Add(newTopic);
        await _context.SaveChangesAsync();

        TempData["SuccessMessage"] = "Pomyślnie utworzono nowy temat!";

        return RedirectToAction(nameof(Topic), new { id = newTopic.Id });
    }


    [HttpPost]
    public async Task<IActionResult> CreatePost(Guid forumTopicId, string content)
    {
        if (string.IsNullOrWhiteSpace(content))
        {
            TempData["ErrorMessage"] = "Treść odpowiedzi nie może być pusta.";
            return RedirectToAction(nameof(Topic), new { id = forumTopicId });
        }

        var user = await _userManager.GetUserAsync(User);

        var post = new ForumPost
        {
            Id = Guid.NewGuid(),
            Content = content,
            CreatedAt = DateTime.Now,
            ForumTopicId = forumTopicId,
            AppUserId = user.Id
        };

        _context.ForumPosts.Add(post);
        await _context.SaveChangesAsync();


        var topic = await _context.ForumTopics.FindAsync(forumTopicId);
        if (topic != null && topic.AppUserId != user.Id)
        {
            await _notificationService.SendNotificationAsync(
                topic.AppUserId,
                $"{user.FirstName} {user.LastName} odpowiedział(a) na Twój temat: {topic.Title}"
            );
        }


        var participantIds = await _context.ForumPosts
            .Where(p => p.ForumTopicId == forumTopicId && p.AppUserId != user.Id && p.AppUserId != topic.AppUserId)
            .Select(p => p.AppUserId)
            .Distinct()
            .ToListAsync();

        foreach (var participantId in participantIds)
        {
            await _notificationService.SendNotificationAsync(
                participantId,
                $"Nowa odpowiedź w temacie: {topic.Title}"
            );
        }

        TempData["SuccessMessage"] = "Dodano odpowiedź!";
        return RedirectToAction(nameof(Topic), new { id = forumTopicId });
    }


    [HttpGet]
    public IActionResult CreateCategory()
    {
        return View();
    }


    [HttpPost]
    public async Task<IActionResult> CreateCategory(string name, string? description)
    {
        if (string.IsNullOrWhiteSpace(name))
        {
            TempData["ErrorMessage"] = "Nazwa forum nie może być pusta!";
            return View();
        }

        var newCategory = new ForumCategory
        {
            Name = name,
            Description = description
        };

        _context.ForumCategories.Add(newCategory);
        await _context.SaveChangesAsync();

        TempData["SuccessMessage"] = "Pomyślnie dodano nowe forum!";
        return RedirectToAction(nameof(Index));
    }


    private async Task SeedForumCategoriesIfNotExists()
    {
        if (!await _context.ForumCategories.AnyAsync())
        {
            _context.ForumCategories.AddRange(
                new ForumCategory { Name = "Historie Sukcesu", Description = "Podziel się tym, jak udało Ci się przezwyciężyć słabości. Motywuj innych!" },
                new ForumCategory { Name = "Sposoby na Głód Hazardowy", Description = "Jakie macie techniki na odwrócenie uwagi, gdy przychodzi ochota na grę?" },
                new ForumCategory { Name = "Luźne Rozmowy", Description = "Miejsce na wszystko inne. Pogadajmy o sporcie (ale bez obstawiania!), filmach i życiu." }
            );
            await _context.SaveChangesAsync();
        }
    }
}