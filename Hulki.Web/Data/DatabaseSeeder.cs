using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Bogus;
using Hulki.Web.Models;
using Hulki.Web.Services;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;

namespace Hulki.Web.Data;

/// <summary>
/// Seeder danych testowych oparty na bibliotece Bogus (Faker).
/// Wywołuj tylko w środowisku deweloperskim (IsDevelopment).
/// </summary>
public static class DatabaseSeeder
{
    private const string DefaultPassword = "Test1234!";
    private const string Locale          = "pl";

    public static async Task SeedAsync(
        ApplicationDbContext db,
        UserManager<AppUser> userManager,
        RoleManager<IdentityRole> roleManager)
    {
        await SeedLookupsAsync(db);

        foreach (var role in new[] { "Admin", "Therapist", "Patient" })
            if (!await roleManager.RoleExistsAsync(role))
                await roleManager.CreateAsync(new IdentityRole(role));

        var therapists = await SeedTherapistsAsync(db, userManager, 5);
        var patients   = await SeedPatientsAsync(db, userManager, 20);

        // własna tabela Users z ręcznym hashowaniem
        await SeedCustomUsersAsync(db, therapists, patients);

        var groups = await SeedTherapyGroupsAsync(db, therapists);
        await SeedPatientGroupsAsync(db, patients, groups);
        await SeedConsultationsAsync(db, therapists, patients);
        await SeedMoodLogsAsync(db, patients);
        await SeedDailyReportsAsync(db, patients);
        await SeedForumAsync(db, patients, therapists);
        await SeedTherapyGoalsAsync(db, patients);

        Console.WriteLine("[DatabaseSeeder] Seeding zakończony.");
    }

    // ──────────────────────────────────────────────────────────────────────

    private static async Task SeedLookupsAsync(ApplicationDbContext db)
    {
        foreach (var t in new[] { "Indywidualna", "Grupowa", "CBT", "Psychodynamiczna", "Gestalt" })
            if (!await db.TherapyTypes.AnyAsync(x => x.Name == t))
                db.TherapyTypes.Add(new TherapyType { Name = t });

        foreach (var s in new[] { "Zaplanowana", "Zakończona", "Anulowana", "Oczekująca" })
            if (!await db.ConsultationStatuses.AnyAsync(x => x.Name == s))
                db.ConsultationStatuses.Add(new ConsultationStatus { Name = s });

        foreach (var s in new[] { "Roboczy", "Wysłany", "Przeczytany" })
            if (!await db.ReportStatuses.AnyAsync(x => x.Name == s))
                db.ReportStatuses.Add(new ReportStatus { Name = s });

        foreach (var m in new[] { "Bardzo dobry", "Dobry", "Neutralny", "Zły", "Bardzo zły" })
            if (!await db.MoodTypes.AnyAsync(x => x.Name == m))
                db.MoodTypes.Add(new MoodType { Name = m });

        foreach (var c in new[] { "Ogólne", "Pytania", "Wsparcie", "Zasoby", "Sukcesy" })
            if (!await db.ForumCategories.AnyAsync(x => x.Name == c))
                db.ForumCategories.Add(new ForumCategory { Name = c });

        await db.SaveChangesAsync();
    }

    private static async Task<List<AppUser>> SeedTherapistsAsync(
        ApplicationDbContext db, UserManager<AppUser> userManager, int count)
    {
        var result = new List<AppUser>();
        var faker  = new Faker(Locale);

        for (int i = 0; i < count; i++)
        {
            var email = $"terapeuta{i + 1}@hulki.pl";
            var existing = await userManager.FindByEmailAsync(email);
            if (existing != null) { result.Add(existing); continue; }

            var user = new AppUser
            {
                FirstName      = faker.Name.FirstName(),
                LastName       = faker.Name.LastName(),
                IsTherapist    = true,
                EmailConfirmed = true,
                Email          = email,
                UserName       = email
            };

            var r = await userManager.CreateAsync(user, DefaultPassword);
            if (r.Succeeded)
            {
                await userManager.AddToRoleAsync(user, "Therapist");
                if (!db.Wallets.Any(w => w.AppUserId == user.Id))
                    db.Wallets.Add(new Wallet { AppUserId = user.Id, Balance = 500 });
                result.Add(user);
            }
        }

        await db.SaveChangesAsync();
        return result;
    }

    private static async Task<List<AppUser>> SeedPatientsAsync(
        ApplicationDbContext db, UserManager<AppUser> userManager, int count)
    {
        var result = new List<AppUser>();
        var faker  = new Faker(Locale);

        for (int i = 0; i < count; i++)
        {
            var email = $"pacjent{i + 1}@hulki.pl";
            var existing = await userManager.FindByEmailAsync(email);
            if (existing != null) { result.Add(existing); continue; }

            var user = new AppUser
            {
                FirstName      = faker.Name.FirstName(),
                LastName       = faker.Name.LastName(),
                IsTherapist    = false,
                EmailConfirmed = true,
                Email          = email,
                UserName       = email
            };

            var r = await userManager.CreateAsync(user, DefaultPassword);
            if (r.Succeeded)
            {
                await userManager.AddToRoleAsync(user, "Patient");
                if (!db.Wallets.Any(w => w.AppUserId == user.Id))
                    db.Wallets.Add(new Wallet { AppUserId = user.Id, Balance = 100 });
                result.Add(user);
            }
        }

        await db.SaveChangesAsync();
        return result;
    }

    /// <summary>
    /// Zapisuje użytkowników do własnej tabeli CustomUsers
    /// z hasłami hashowanymi przez CustomPasswordHasher (PBKDF2-SHA256).
    /// </summary>
    private static async Task SeedCustomUsersAsync(
        ApplicationDbContext db,
        List<AppUser> therapists,
        List<AppUser> patients)
    {
        foreach (var appUser in therapists.Concat(patients))
        {
            if (!await db.CustomUsers.AnyAsync(u => u.Email == appUser.Email))
            {
                db.CustomUsers.Add(new CustomUser
                {
                    FirstName    = appUser.FirstName,
                    LastName     = appUser.LastName,
                    Email        = appUser.Email,
                    PasswordHash = CustomPasswordHasher.Hash(DefaultPassword),
                    IsTherapist  = appUser.IsTherapist,
                    AspNetUserId = appUser.Id,
                    CreatedAt    = DateTime.UtcNow
                });
            }
        }

        await db.SaveChangesAsync();
    }

    private static async Task<List<TherapyGroup>> SeedTherapyGroupsAsync(
        ApplicationDbContext db, List<AppUser> therapists)
    {
        if (await db.TherapyGroups.AnyAsync())
            return await db.TherapyGroups.ToListAsync();

        var therapyTypes = await db.TherapyTypes.ToListAsync();
        var faker        = new Faker(Locale);

        var names = new[]
        {
            "Grupa wsparcia – depresja",
            "Radzenie sobie ze stresem",
            "Warsztaty uważności",
            "Lęki i fobie",
            "Relacje interpersonalne",
            "Zarządzanie gniewem",
            "Terapia traumy",
            "Wsparcie po stracie"
        };

        var groups = new List<TherapyGroup>();
        foreach (var name in names)
        {
            var g = new TherapyGroup
            {
                Name            = name,
                Description     = faker.Lorem.Sentence(10),
                MaxParticipants = faker.Random.Int(5, 15),
                TherapyTypeId   = faker.PickRandom(therapyTypes).Id
            };
            db.TherapyGroups.Add(g);
            groups.Add(g);
        }

        await db.SaveChangesAsync();
        return groups;
    }

    private static async Task SeedPatientGroupsAsync(
        ApplicationDbContext db, List<AppUser> patients, List<TherapyGroup> groups)
    {
        if (await db.PatientGroups.AnyAsync()) return;

        var faker = new Faker();
        foreach (var patient in patients)
        {
            var selected = faker.PickRandom(groups, faker.Random.Int(1, 3)).Distinct();
            foreach (var group in selected)
                db.PatientGroups.Add(new PatientGroup
                {
                    AppUserId      = patient.Id,
                    TherapyGroupId = group.Id,
                    JoinedDate     = faker.Date.Recent(90),
                    IsApproved     = true
                });
        }

        await db.SaveChangesAsync();
    }

    private static async Task SeedConsultationsAsync(
        ApplicationDbContext db, List<AppUser> therapists, List<AppUser> patients)
    {
        if (await db.Consultations.AnyAsync()) return;

        var statuses = await db.ConsultationStatuses.ToListAsync();
        var faker    = new Faker(Locale);

        for (int i = 0; i < 40; i++)
        {
            var start = faker.Date.Between(DateTime.Now.AddMonths(-6), DateTime.Now.AddMonths(2));
            db.Consultations.Add(new Consultation
            {
                Id          = Guid.NewGuid(),
                TherapistId = faker.PickRandom(therapists).Id,
                PatientId   = faker.PickRandom(patients).Id,
                StartTime   = start,
                EndTime     = start.AddHours(1),
                Notes       = faker.Lorem.Paragraph(),
                StatusId    = faker.PickRandom(statuses).Id
            });
        }

        await db.SaveChangesAsync();
    }

    private static async Task SeedMoodLogsAsync(ApplicationDbContext db, List<AppUser> patients)
    {
        if (await db.MoodLogs.AnyAsync()) return;

        var moodTypes = await db.MoodTypes.ToListAsync();
        var faker     = new Faker();

        foreach (var patient in patients)
            for (int d = 0; d < 30; d++)
                db.MoodLogs.Add(new MoodLog
                {
                    Id         = Guid.NewGuid(),
                    AppUserId  = patient.Id,
                    Date       = DateTime.Now.AddDays(-d),
                    MoodTypeId = faker.PickRandom(moodTypes).Id
                });

        await db.SaveChangesAsync();
    }

    private static async Task SeedDailyReportsAsync(ApplicationDbContext db, List<AppUser> patients)
    {
        if (await db.DailyReports.AnyAsync()) return;

        var reportStatuses = await db.ReportStatuses.ToListAsync();
        var faker          = new Faker(Locale);

        foreach (var patient in patients)
            for (int d = 0; d < 10; d++)
                db.DailyReports.Add(new DailyReport
                {
                    Id             = Guid.NewGuid(),
                    AppUserId      = patient.Id,
                    Content        = faker.Lorem.Paragraphs(2),
                    CreatedAt      = DateTime.Now.AddDays(-d),
                    ReportStatusId = faker.PickRandom(reportStatuses).Id
                });

        await db.SaveChangesAsync();
    }

    private static async Task SeedForumAsync(
        ApplicationDbContext db, List<AppUser> patients, List<AppUser> therapists)
    {
        if (await db.ForumTopics.AnyAsync()) return;

        var categories = await db.ForumCategories.ToListAsync();
        var allUsers   = patients.Concat(therapists).ToList();
        var faker      = new Faker(Locale);

        for (int t = 0; t < 15; t++)
        {
            var topic = new ForumTopic
            {
                Id              = Guid.NewGuid(),
                Title           = faker.Lorem.Sentence(5),
                Content         = faker.Lorem.Paragraphs(2),
                CreatedAt       = faker.Date.Between(DateTime.Now.AddMonths(-3), DateTime.Now),
                AppUserId       = faker.PickRandom(allUsers).Id,
                ForumCategoryId = faker.PickRandom(categories).Id
            };
            db.ForumTopics.Add(topic);

            for (int p = 0; p < faker.Random.Int(2, 8); p++)
                db.ForumPosts.Add(new ForumPost
                {
                    Id           = Guid.NewGuid(),
                    Content      = faker.Lorem.Paragraphs(1),
                    CreatedAt    = topic.CreatedAt.AddHours(faker.Random.Int(1, 72)),
                    AppUserId    = faker.PickRandom(allUsers).Id,
                    ForumTopicId = topic.Id
                });
        }

        // ── "Gorący" wątek z ~150 postami – do testowania paginacji ─────────
        var hotTopic = new ForumTopic
        {
            Id              = Guid.NewGuid(),
            Title           = "Jak radzicie sobie z głodem hazardowym wieczorami?",
            Content         = faker.Lorem.Paragraphs(2),
            CreatedAt       = DateTime.Now.AddMonths(-2),
            AppUserId       = faker.PickRandom(allUsers).Id,
            ForumCategoryId = faker.PickRandom(categories).Id
        };
        db.ForumTopics.Add(hotTopic);

        int hotPostCount = faker.Random.Int(140, 160);
        for (int p = 0; p < hotPostCount; p++)
            db.ForumPosts.Add(new ForumPost
            {
                Id           = Guid.NewGuid(),
                Content      = faker.Lorem.Paragraphs(1),
                CreatedAt    = hotTopic.CreatedAt.AddHours(p), // narastająco -> zachowuje kolejność chronologiczną
                AppUserId    = faker.PickRandom(allUsers).Id,
                ForumTopicId = hotTopic.Id
            });

        await db.SaveChangesAsync();
    }

    private static async Task SeedTherapyGoalsAsync(ApplicationDbContext db, List<AppUser> patients)
    {
        if (await db.TherapyGoals.AnyAsync()) return;

        var faker = new Faker(Locale);
        var goalTitles = new[]
        {
            "Redukcja poziomu lęku", "Poprawa relacji rodzinnych",
            "Nauka technik relaksacyjnych", "Regularne ćwiczenia fizyczne",
            "Zdrowy sen", "Asertywność w pracy",
            "Radzenie sobie z krytyką", "Budowanie pewności siebie"
        };

        foreach (var patient in patients)
        {
            for (int g = 0; g < faker.Random.Int(1, 3); g++)
            {
                var goal = new TherapyGoal
                {
                    Id          = Guid.NewGuid(),
                    AppUserId   = patient.Id,
                    Title       = faker.PickRandom(goalTitles),
                    Description = faker.Lorem.Sentence(8),
                    Deadline    = DateTime.Now.AddMonths(faker.Random.Int(1, 6)),
                    IsCompleted = faker.Random.Bool(0.2f)
                };
                db.TherapyGoals.Add(goal);

                for (int m = 0; m < faker.Random.Int(2, 4); m++)
                    db.GoalMilestones.Add(new GoalMilestone
                    {
                        Id          = Guid.NewGuid(),
                        GoalId      = goal.Id,          // poprawne pole z modelu
                        Description = faker.Lorem.Sentence(4),  // poprawne pole z modelu
                        IsCompleted = faker.Random.Bool(0.3f)
                    });
            }
        }

        await db.SaveChangesAsync();
    }
}