using Hulki.Web.Data;
using Hulki.Web.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System;
using System.Linq;
using System.Threading.Tasks;

namespace Hulki.Web.Controllers;

[Authorize(Roles = "Admin")] 
public class AdminController : Controller
{
    private readonly ApplicationDbContext _context;

    public AdminController(ApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<IActionResult> Index()
    {
        var today = DateTime.Today;

        var totalUsers = await _context.Users.CountAsync();
        var totalPatients = totalUsers > 0 ? totalUsers - 1 : 0;

        var reportsToday = await _context.DailyReports
            .Where(r => r.CreatedAt.Date == today)
            .CountAsync();

        var totalGroups = await _context.TherapyGroups.CountAsync();

        var recentReports = await _context.DailyReports
            .Include(r => r.AppUser)
            .Include(r => r.ReportStatus)
            .OrderByDescending(r => r.CreatedAt)
            .Take(5)
            .ToListAsync();

        ViewBag.TotalPatients = totalPatients;
        ViewBag.ReportsToday = reportsToday;
        ViewBag.TotalGroups = totalGroups;
        ViewBag.RecentReports = recentReports;

        return View();
    }
}