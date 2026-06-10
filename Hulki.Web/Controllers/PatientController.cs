using Hulki.Web.Data;
using Hulki.Web.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;

namespace Hulki.Web.Controllers;

// Tylko zalogowani
    [Authorize]
    public class PatientController : Controller
    {
        private readonly ApplicationDbContext _context;
        private readonly UserManager<AppUser> _userManager;
        private readonly IWebHostEnvironment _webHostEnvironment;

        public PatientController(ApplicationDbContext context, UserManager<AppUser> userManager, IWebHostEnvironment webHostEnvironment)
        {
            _context = context;
            _userManager = userManager;
            _webHostEnvironment = webHostEnvironment;
        }

        // PANEL PACJENTA Z WYKRESAMI POSTĘPU
        [HttpGet]
        public async Task<IActionResult> Index()
        {
            var user = await _userManager.GetUserAsync(User);
            if (user == null) return RedirectToAction("Login", "Account");

            // --- Raporty z ostatnich 30 dni ---
            var thirtyDaysAgo = DateTime.Today.AddDays(-29);
            var reports = await _context.DailyReports
                .Where(r => r.AppUserId == user.Id && r.CreatedAt >= thirtyDaysAgo)
                .OrderBy(r => r.CreatedAt)
                .ToListAsync();

            // Dane dla wykresu aktywności (liczba raportów per dzień)
            var last30Days = Enumerable.Range(0, 30)
                .Select(i => DateTime.Today.AddDays(-29 + i))
                .ToList();

            var activityLabels = last30Days.Select(d => d.ToString("dd.MM")).ToList();
            var activityData = last30Days.Select(d =>
                reports.Count(r => r.CreatedAt.Date == d.Date)).ToList();

            // --- Transakcje punktowe (ostatnie 30 dni) ---
            var wallet = await _context.Wallets
                .Include(w => w.AppUser)
                .FirstOrDefaultAsync(w => w.AppUserId == user.Id);

            List<PointTransaction> transactions = new();
            if (wallet != null)
            {
                transactions = await _context.PointTransactions
                    .Where(t => t.WalletId == wallet.Id && t.TransactionDate >= thirtyDaysAgo)
                    .OrderBy(t => t.TransactionDate)
                    .ToListAsync();
            }

            // Dane dla wykresu skumulowanych punktów (bieżące saldo każdego dnia)
            var currentBalance = wallet?.Balance ?? 0;
            var totalPointsInPeriod = transactions.Sum(t => t.Amount);
            var balanceBeforePeriod = currentBalance - totalPointsInPeriod;

            var pointsLabels = last30Days.Select(d => d.ToString("dd.MM")).ToList();
            var pointsData = new List<int>();
            var runningBalance = balanceBeforePeriod;
            foreach (var day in last30Days)
            {
                runningBalance += transactions
                    .Where(t => t.TransactionDate.Date == day.Date)
                    .Sum(t => t.Amount);
                pointsData.Add(runningBalance);
            }

            // --- Streak (seria kolejnych dni z raportem) ---
            var allReports = await _context.DailyReports
                .Where(r => r.AppUserId == user.Id)
                .Select(r => r.CreatedAt.Date)
                .Distinct()
                .OrderByDescending(d => d)
                .ToListAsync();

            int streak = 0;
            var checkDate = DateTime.Today;
            foreach (var reportDate in allReports)
            {
                if (reportDate == checkDate || reportDate == checkDate.AddDays(-1))
                {
                    streak++;
                    checkDate = reportDate;
                }
                else break;
            }

            // --- Łączna liczba raportów i procent aktywności ---
            var totalReports = await _context.DailyReports.CountAsync(r => r.AppUserId == user.Id);
            var reportsThisMonth = reports.Count;
            var daysInMonth = DateTime.Today.Day;
            var activityPercent = daysInMonth > 0 ? (int)Math.Round((double)reportsThisMonth / daysInMonth * 100) : 0;

            ViewBag.ActivityLabels = activityLabels;
            ViewBag.ActivityData = activityData;
            ViewBag.PointsLabels = pointsLabels;
            ViewBag.PointsData = pointsData;
            ViewBag.CurrentBalance = currentBalance;
            ViewBag.Streak = streak;
            ViewBag.TotalReports = totalReports;
            ViewBag.ReportsThisMonth = reportsThisMonth;
            ViewBag.ActivityPercent = activityPercent;
            ViewBag.UserName = user.FirstName ?? user.UserName;

            return View();
        }

    // W PatientController.cs
    [HttpGet]
    public async Task<IActionResult> AddMood()
    {
        // Pobieramy typy nastrojów, żeby wyświetlić je w dropdownie
        ViewBag.MoodTypes = await _context.MoodTypes.ToListAsync();
        return View();
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> AddMood(int moodTypeId)
    {
        var userId = _userManager.GetUserId(User); // Pobieramy ID zalogowanego pacjenta

        var log = new MoodLog
        {
            Id = Guid.NewGuid(),
            AppUserId = userId,
            Date = DateTime.Now,
            MoodTypeId = moodTypeId
        };

        _context.MoodLogs.Add(log);
        await _context.SaveChangesAsync();

        TempData["SuccessMessage"] = "Twój nastrój został zapisany!";
        return RedirectToAction("Index"); // Wracamy do panelu głównego pacjenta
    }

    // 1. WYŚWIETLANIE FORMULARZA
    [HttpGet]
        public async Task<IActionResult> DailyReport()
        {
            var user = await _userManager.GetUserAsync(User);
            var today = DateTime.Today;
            bool alreadyReportedToday = await _context.DailyReports
                .AnyAsync(r => r.AppUserId == user.Id && r.CreatedAt >= today && r.CreatedAt < today.AddDays(1));
            ViewBag.AlreadyReportedToday = alreadyReportedToday;
            return View();
        }

        // 2. PRZETWARZANIE WYSŁANEGO FORMULARZA (Z PLIKIEM)
        [HttpPost]
        public async Task<IActionResult> DailyReport(string content, IFormFile? attachment)
        {
            if (string.IsNullOrWhiteSpace(content))
            {
                ModelState.AddModelError("", "Treść raportu nie może być pusta!");
                return View();
            }

            var user = await _userManager.GetUserAsync(User);

            // LIMIT: jeden raport dziennie
            var today = DateTime.Today;
            bool alreadyReportedToday = await _context.DailyReports
                .AnyAsync(r => r.AppUserId == user.Id && r.CreatedAt >= today && r.CreatedAt < today.AddDays(1));

            if (alreadyReportedToday)
            {
                TempData["ErrorMessage"] = "Już dodałeś dzisiaj swój wpis do dzienniczka. Wróć jutro!";
                return RedirectToAction("DailyReport");
            }

            // SPRAWDZANIE SŁOWNIKÓW
            var defaultStatus = await _context.ReportStatuses.FirstOrDefaultAsync(s => s.Name == "Oczekujący") 
                                ?? new ReportStatus { Name = "Oczekujący" };
            if (defaultStatus.Id == 0) _context.ReportStatuses.Add(defaultStatus);

            // ZAPIS RAPORTU
            var report = new DailyReport
            {
                Id = Guid.NewGuid(),
                Content = content,
                CreatedAt = DateTime.Now,
                AppUserId = user.Id,
                ReportStatus = defaultStatus
            };
            _context.DailyReports.Add(report);

            // UPLOAD
            bool hasAttachment = false; 
            if (attachment != null && attachment.Length > 0)
            {
                var allowedExtensions = new[] { ".jpg", ".jpeg", ".png", ".pdf" };
                var fileExtension = Path.GetExtension(attachment.FileName).ToLowerInvariant();
                if (!allowedExtensions.Contains(fileExtension))
                {
                    ModelState.AddModelError("", "Nieobsługiwany format pliku. Obsługiwane rozszerzenia to: .jpg, .jpeg, .png, .pdf");
                    return View();
                }

                hasAttachment = true;
                var fileType = await _context.FileTypes.FirstOrDefaultAsync(f => f.Name == "Dokument") 
                               ?? new FileType { Name = "Dokument", Extension = Path.GetExtension(attachment.FileName) };
                if (fileType.Id == 0) _context.FileTypes.Add(fileType);

                string uniqueFileName = Guid.NewGuid().ToString() + "_" + attachment.FileName;
                string uploadsFolder = Path.Combine(_webHostEnvironment.WebRootPath, "uploads");
                
                if (!Directory.Exists(uploadsFolder)) Directory.CreateDirectory(uploadsFolder);

                string filePath = Path.Combine(uploadsFolder, uniqueFileName);

                using (var fileStream = new FileStream(filePath, FileMode.Create))
                {
                    await attachment.CopyToAsync(fileStream);
                }

                var reportAttachment = new ReportAttachment
                {
                    Id = Guid.NewGuid(),
                    FileName = attachment.FileName,
                    FilePath = "/uploads/" + uniqueFileName,
                    DailyReportId = report.Id,
                    FileType = fileType
                };
                _context.ReportAttachments.Add(reportAttachment);
            }

            // DODAWANIE PUNKTÓW 
            var wallet = await _context.Wallets.FirstOrDefaultAsync(w => w.AppUserId == user.Id);
            
            if (wallet == null)
            {
                wallet = new Wallet { Id = Guid.NewGuid(), AppUserId = user.Id, Balance = 0 };
                _context.Wallets.Add(wallet);
                await _context.SaveChangesAsync(); 
            }

            int pointsEarned = 10; 
            string transactionReason = "Nagroda za dzienny wpis";

            if (hasAttachment)
            {
                pointsEarned += 5; 
                transactionReason += " (+5 pkt za dodanie załącznika)";
            }

            wallet.Balance += pointsEarned; 

            var transaction = new PointTransaction
            {
                Id = Guid.NewGuid(),
                Amount = pointsEarned,
                Description = transactionReason,
                TransactionDate = DateTime.Now,
                WalletId = wallet.Id
            };
            _context.PointTransactions.Add(transaction);

            await _context.SaveChangesAsync();

            TempData["SuccessMessage"] = $"Świetna robota! Raport dodany, a na twoje konto wpłynęło +{pointsEarned} punktów!";
            return RedirectToAction("Index", "Home");
        }
    }