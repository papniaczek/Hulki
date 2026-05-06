using Hulki.Web.Data;
using Hulki.Web.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System;
using System.IO;
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
            bool hasAttachment = false; // Flaga, czy pacjent dodał plik
            if (attachment != null && attachment.Length > 0)
            {
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

            // --- DODAWANIE PUNKTÓW (ULEPSZONE POD HARMONOGRAM) ---
            var wallet = await _context.Wallets.FirstOrDefaultAsync(w => w.AppUserId == user.Id);
            
            // Jeśli jakimś cudem nie ma portfela
            if (wallet == null)
            {
                wallet = new Wallet { Id = Guid.NewGuid(), AppUserId = user.Id, Balance = 0 };
                _context.Wallets.Add(wallet);
                await _context.SaveChangesAsync(); // Zapisujemy nowy portfel, żeby wygenerował się w bazie
            }

            // Logika dodatkowych aktywności!
            int pointsEarned = 10; // Baza za napisanie czegokolwiek
            string transactionReason = "Nagroda za dzienny wpis";

            if (hasAttachment)
            {
                pointsEarned += 5; // Dodatkowe punkty za dodanie załącznika
                transactionReason += " (+5 pkt za dodanie załącznika)";
            }

            wallet.Balance += pointsEarned; 

            var transaction = new PointTransaction
            {
                Id = Guid.NewGuid(),
                Amount = pointsEarned,
                Description = transactionReason, // Używam Twojego pola Description
                TransactionDate = DateTime.Now,
                WalletId = wallet.Id
            };
            _context.PointTransactions.Add(transaction);

            await _context.SaveChangesAsync();

            TempData["SuccessMessage"] = $"Świetna robota! Raport dodany, a na Twoje konto wpłynęło +{pointsEarned} punktów!";
            return RedirectToAction("Index", "Home");
        }
    }