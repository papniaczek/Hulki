using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using Hulki.Web.Data;
using Hulki.Web.Models;
using Microsoft.EntityFrameworkCore;
using QuestPDF.Fluent;
using QuestPDF.Helpers;
using QuestPDF.Infrastructure;

namespace Hulki.Web.Services
{
    public interface IPdfReportService
    {
        Task<byte[]> GenerateDailyReportsPdfAsync(
            string userId,
            DateTime? startDate = null,
            DateTime? endDate = null
        );
        Task<byte[]> GenerateUserProgressReportAsync(string userId);
        Task<byte[]> GenerateConsultationReportAsync(Guid consultationId);
    }

    public class PdfReportService : IPdfReportService
    {
        private readonly ApplicationDbContext _context;

        public PdfReportService(ApplicationDbContext context)
        {
            _context = context;

            QuestPDF.Settings.License = LicenseType.Community;
        }

        public async Task<byte[]> GenerateDailyReportsPdfAsync(
            string userId,
            DateTime? startDate = null,
            DateTime? endDate = null
        )
        {
            var user = await _context.Users.FindAsync(userId);
            if (user == null)
                throw new InvalidOperationException("Użytkownik nie znaleziony");

            var query = _context
                .DailyReports.Include(r => r.ReportAttachments)
                .Where(r => r.AppUserId == userId);

            if (startDate.HasValue)
                query = query.Where(r => r.CreatedAt >= startDate.Value);

            if (endDate.HasValue)
                query = query.Where(r => r.CreatedAt <= endDate.Value.AddDays(1));

            var reports = await query.OrderByDescending(r => r.CreatedAt).ToListAsync();

            var document = Document.Create(container =>
            {
                container.Page(page =>
                {
                    page.Size(PageSizes.A4);
                    page.Margin(2, Unit.Centimetre);
                    page.DefaultTextStyle(x => x.FontSize(11).FontFamily("Arial"));

                    page.Header().Element(Header);
                    page.Content()
                        .Element(content => Content(content, user, reports, startDate, endDate));
                    page.Footer().Element(Footer);
                });
            });

            return document.GeneratePdf();
        }

        public async Task<byte[]> GenerateUserProgressReportAsync(string userId)
        {
            var user = await _context
                .Users.Include(u => u.Wallet)
                .FirstOrDefaultAsync(u => u.Id == userId);

            if (user == null)
                throw new InvalidOperationException("Użytkownik nie znaleziony");

            var goals = await _context
                .TherapyGoals.Include(g => g.Milestones)
                .Where(g => g.AppUserId == userId)
                .OrderByDescending(g => g.Deadline)
                .ToListAsync();

            var badges = await _context
                .UserBadges.Include(ub => ub.Badge)
                .Where(ub => ub.AppUserId == userId)
                .OrderByDescending(ub => ub.EarnedAt)
                .ToListAsync();

            var reportsCount = await _context.DailyReports.CountAsync(r => r.AppUserId == userId);
            var consultationsCount = await _context.Consultations.CountAsync(c =>
                c.PatientId == userId && c.Status.Name == "Zakończona"
            );

            var document = Document.Create(container =>
            {
                container.Page(page =>
                {
                    page.Size(PageSizes.A4);
                    page.Margin(2, Unit.Centimetre);
                    page.DefaultTextStyle(x => x.FontSize(11).FontFamily("Arial"));

                    page.Header().Element(Header);
                    page.Content()
                        .Element(content =>
                            ProgressContent(
                                content,
                                user,
                                goals,
                                badges,
                                reportsCount,
                                consultationsCount
                            )
                        );
                    page.Footer().Element(Footer);
                });
            });

            return document.GeneratePdf();
        }

        public async Task<byte[]> GenerateConsultationReportAsync(Guid consultationId)
        {
            var consultation = await _context
                .Consultations.Include(c => c.Patient)
                .Include(c => c.Therapist)
                .Include(c => c.Status)
                .FirstOrDefaultAsync(c => c.Id == consultationId);

            if (consultation == null)
                throw new InvalidOperationException("Konsultacja nie znaleziona");

            var document = Document.Create(container =>
            {
                container.Page(page =>
                {
                    page.Size(PageSizes.A4);
                    page.Margin(2, Unit.Centimetre);
                    page.DefaultTextStyle(x => x.FontSize(11).FontFamily("Arial"));

                    page.Header().Element(Header);
                    page.Content().Element(content => ConsultationContent(content, consultation));
                    page.Footer().Element(Footer);
                });
            });

            return document.GeneratePdf();
        }

        private void Header(IContainer container)
        {
            container.Row(row =>
            {
                row.RelativeItem()
                    .Column(column =>
                    {
                        column
                            .Item()
                            .Text("HULKI - System Terapeutyczny")
                            .FontSize(20)
                            .Bold()
                            .FontColor(Colors.Blue.Darken3);

                        column
                            .Item()
                            .Text($"Data wygenerowania: {DateTime.Now:dd.MM.yyyy HH:mm}")
                            .FontSize(9)
                            .FontColor(Colors.Grey.Darken2);
                    });

                row.ConstantItem(100)
                    .Column(col =>
                    {
                        col.Item().Text("📋").FontSize(40);
                    });
            });
        }

        private void Footer(IContainer container)
        {
            container
                .AlignCenter()
                .Text(text =>
                {
                    text.DefaultTextStyle(style =>
                        style.FontSize(9).FontColor(Colors.Grey.Darken1)
                    );
                    text.Span("Strona ");
                    text.CurrentPageNumber();
                    text.Span(" z ");
                    text.TotalPages();
                });
        }

        private void Content(
            IContainer container,
            AppUser user,
            List<DailyReport> reports,
            DateTime? startDate,
            DateTime? endDate
        )
        {
            container
                .PaddingVertical(10)
                .Column(column =>
                {
                    column.Spacing(10);

                    column
                        .Item()
                        .Background(Colors.Blue.Lighten4)
                        .Padding(10)
                        .Column(header =>
                        {
                            header
                                .Item()
                                .Text($"Raporty dzienne - {user.FirstName} {user.LastName}")
                                .FontSize(16)
                                .Bold();

                            if (startDate.HasValue || endDate.HasValue)
                            {
                                var period =
                                    $"Okres: {startDate?.ToString("dd.MM.yyyy") ?? "początek"} - {endDate?.ToString("dd.MM.yyyy") ?? "dziś"}";
                                header.Item().Text(period).FontSize(10);
                            }

                            header.Item().Text($"Liczba raportów: {reports.Count}").FontSize(10);
                        });

                    column.Item().PaddingTop(10);

                    if (!reports.Any())
                    {
                        column
                            .Item()
                            .Text("Brak raportów w wybranym okresie.")
                            .FontSize(12)
                            .Italic()
                            .FontColor(Colors.Grey.Darken1);
                    }
                    else
                    {
                        foreach (var report in reports)
                        {
                            column
                                .Item()
                                .BorderBottom(1)
                                .BorderColor(Colors.Grey.Lighten2)
                                .PaddingVertical(10)
                                .Column(reportColumn =>
                                {
                                    reportColumn
                                        .Item()
                                        .Row(row =>
                                        {
                                            row.RelativeItem()
                                                .Text($"📅 {report.CreatedAt:dd MMMM yyyy, HH:mm}")
                                                .Bold()
                                                .FontSize(12);
                                        });

                                    reportColumn
                                        .Item()
                                        .PaddingTop(5)
                                        .Text(report.Content)
                                        .FontSize(10)
                                        .LineHeight(1.4f);

                                    if (report.ReportAttachments?.Any() == true)
                                    {
                                        reportColumn
                                            .Item()
                                            .PaddingTop(5)
                                            .Text(
                                                $"📎 Załączniki: {string.Join(", ", report.ReportAttachments.Select(a => a.FileName))}"
                                            )
                                            .FontSize(9)
                                            .Italic()
                                            .FontColor(Colors.Grey.Darken1);
                                    }
                                });
                        }
                    }
                });
        }

        private void ProgressContent(
            IContainer container,
            AppUser user,
            List<TherapyGoal> goals,
            List<UserBadge> badges,
            int reportsCount,
            int consultationsCount
        )
        {
            container
                .PaddingVertical(10)
                .Column(column =>
                {
                    column.Spacing(15);

                    column
                        .Item()
                        .Background(Colors.Green.Lighten4)
                        .Padding(10)
                        .Column(header =>
                        {
                            header
                                .Item()
                                .Text($"Raport postępów - {user.FirstName} {user.LastName}")
                                .FontSize(16)
                                .Bold();
                        });

                    column
                        .Item()
                        .Background(Colors.Grey.Lighten4)
                        .Padding(10)
                        .Column(stats =>
                        {
                            stats.Item().Text("📊 Podsumowanie aktywności").FontSize(14).Bold();
                            stats
                                .Item()
                                .PaddingTop(5)
                                .Row(row =>
                                {
                                    row.RelativeItem()
                                        .Text($"💰 Punkty: {user.Wallet?.Balance ?? 0}");
                                    row.RelativeItem().Text($"📝 Raporty dzienne: {reportsCount}");
                                });
                            stats
                                .Item()
                                .PaddingTop(3)
                                .Row(row =>
                                {
                                    row.RelativeItem()
                                        .Text(
                                            $"🎯 Cele ukończone: {goals.Count(g => g.IsCompleted)}/{goals.Count}"
                                        );
                                    row.RelativeItem()
                                        .Text($"💬 Konsultacje: {consultationsCount}");
                                });
                            stats.Item().PaddingTop(3).Text($"🏆 Odznaki: {badges.Count}");
                        });

                    column
                        .Item()
                        .PaddingTop(10)
                        .Column(goalsSection =>
                        {
                            goalsSection.Item().Text("🎯 Cele terapeutyczne").FontSize(14).Bold();

                            if (!goals.Any())
                            {
                                goalsSection.Item().Text("Brak celów terapeutycznych.").Italic();
                            }
                            else
                            {
                                foreach (var goal in goals.Take(10))
                                {
                                    goalsSection
                                        .Item()
                                        .PaddingTop(5)
                                        .Column(goalBox =>
                                        {
                                            goalBox
                                                .Item()
                                                .Row(row =>
                                                {
                                                    row.AutoItem()
                                                        .Text(goal.IsCompleted ? "✅" : "⏳");
                                                    row.RelativeItem()
                                                        .PaddingLeft(5)
                                                        .Text(goal.Title)
                                                        .Bold();
                                                });
                                            if (!string.IsNullOrWhiteSpace(goal.Description))
                                            {
                                                goalBox
                                                    .Item()
                                                    .PaddingLeft(15)
                                                    .Text(goal.Description)
                                                    .FontSize(9);
                                            }

                                            if (goal.Milestones?.Any() == true)
                                            {
                                                var completed = goal.Milestones.Count(m =>
                                                    m.IsCompleted
                                                );
                                                var total = goal.Milestones.Count;
                                                goalBox
                                                    .Item()
                                                    .PaddingLeft(15)
                                                    .Text($"Kamienie milowe: {completed}/{total}")
                                                    .FontSize(9)
                                                    .FontColor(Colors.Grey.Darken1);
                                            }
                                        });
                                }
                            }
                        });

                    column
                        .Item()
                        .PaddingTop(10)
                        .Column(badgesSection =>
                        {
                            badgesSection.Item().Text("🏆 Zdobyte odznaki").FontSize(14).Bold();

                            if (!badges.Any())
                            {
                                badgesSection.Item().Text("Brak zdobytych odznak.").Italic();
                            }
                            else
                            {
                                badgesSection
                                    .Item()
                                    .PaddingTop(5)
                                    .Column(badgesList =>
                                    {
                                        foreach (var userBadge in badges)
                                        {
                                            badgesList
                                                .Item()
                                                .PaddingVertical(3)
                                                .Row(row =>
                                                {
                                                    row.AutoItem().Text("🏅");
                                                    row.RelativeItem()
                                                        .PaddingLeft(5)
                                                        .Text(userBadge.Badge.Name)
                                                        .Bold();
                                                    row.AutoItem()
                                                        .Text($"{userBadge.EarnedAt:dd.MM.yyyy}")
                                                        .FontSize(9)
                                                        .FontColor(Colors.Grey.Darken1);
                                                });
                                            badgesList
                                                .Item()
                                                .PaddingLeft(20)
                                                .Text(userBadge.Badge.Description)
                                                .FontSize(9)
                                                .FontColor(Colors.Grey.Darken1);
                                        }
                                    });
                            }
                        });
                });
        }

        private void ConsultationContent(IContainer container, Consultation consultation)
        {
            container
                .PaddingVertical(10)
                .Column(column =>
                {
                    column.Spacing(10);

                    column
                        .Item()
                        .Background(Colors.Purple.Lighten4)
                        .Padding(10)
                        .Column(header =>
                        {
                            header.Item().Text("Raport konsultacji").FontSize(16).Bold();
                            header
                                .Item()
                                .Text($"Data: {consultation.StartTime:dd MMMM yyyy, HH:mm}")
                                .FontSize(10);
                        });

                    column
                        .Item()
                        .Background(Colors.Grey.Lighten4)
                        .Padding(10)
                        .Column(info =>
                        {
                            info.Item().Text("📋 Informacje podstawowe").FontSize(12).Bold();
                            info.Item()
                                .PaddingTop(5)
                                .Text(
                                    $"Pacjent: {consultation.Patient.FirstName} {consultation.Patient.LastName}"
                                );
                            info.Item()
                                .Text(
                                    $"Terapeuta: {consultation.Therapist.FirstName} {consultation.Therapist.LastName}"
                                );
                            info.Item().Text($"Status: {consultation.Status.Name}");
                            info.Item().Text($"Typ: Online");
                        });

                    if (!string.IsNullOrWhiteSpace(consultation.Notes))
                    {
                        column
                            .Item()
                            .PaddingTop(10)
                            .Column(topic =>
                            {
                                topic.Item().Text("📝 Notatki z konsultacji").FontSize(12).Bold();
                                topic.Item().PaddingTop(5).Text(consultation.Notes);
                            });
                    }

                    if (!string.IsNullOrWhiteSpace(consultation.Details?.Recommendations))
                    {
                        column
                            .Item()
                            .PaddingTop(10)
                            .Column(notes =>
                            {
                                notes.Item().Text("📄 Zalecenia").FontSize(12).Bold();
                                notes
                                    .Item()
                                    .PaddingTop(5)
                                    .Text(consultation.Details.Recommendations);
                            });
                    }
                });
        }
    }
}
