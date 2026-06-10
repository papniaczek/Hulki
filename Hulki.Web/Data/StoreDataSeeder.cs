using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Hulki.Web.Models;
using Microsoft.EntityFrameworkCore;

namespace Hulki.Web.Data;

/// <summary>
/// Seeder danych sklepu. 
/// Uruchamiany przy starcie aplikacji (Program.cs), wypełnia bazę danymi testowymi i przykładowymi nagrodami, grami i rzadkościami.
/// </summary>
public static class StoreDataSeeder
{
    public static async Task SeedAsync(ApplicationDbContext context)
    {
        // 1. Rzadkości
        var expectedRarities = new Dictionary<string, string>
        {
            { "Pospolity", "#adb5bd" }, { "Niepospolity", "#198754" }, { "Rzadki", "#0d6efd" },
            { "Epicki", "#6f42c1" }, { "Legendarny", "#ffc107" }, { "Mityczny", "#dc3545" }
        };

        foreach (var rarity in expectedRarities)
        {
            if (!await context.ItemRarities.AnyAsync(r => r.Name == rarity.Key))
                context.ItemRarities.Add(new ItemRarity { Name = rarity.Key, HexColor = rarity.Value });
        }
        await context.SaveChangesAsync();

        // 2. Typy gier
        string[] types = { "Lootbox", "3 Karty", "Zdrapka" };
        foreach (var t in types)
        {
            if (!await context.GameTypes.AnyAsync(gt => gt.Name == t))
                context.GameTypes.Add(new GameType { Name = t, Description = $"Gra typu {t}" });
        }
        await context.SaveChangesAsync();

        // 3. Gry
        var oldGame = await context.Games.FirstOrDefaultAsync(g => g.Name == "Skrzynia Motywacji");
        if (oldGame != null) { oldGame.Name = "Ruletka Nagród"; context.Update(oldGame); }

        var lootType    = await context.GameTypes.FirstOrDefaultAsync(t => t.Name == "Lootbox");
        var cardsType   = await context.GameTypes.FirstOrDefaultAsync(t => t.Name == "3 Karty");
        var scratchType = await context.GameTypes.FirstOrDefaultAsync(t => t.Name == "Zdrapka");

        if (!await context.Games.AnyAsync(g => g.Name == "Ruletka Nagród") && lootType != null)
            context.Games.Add(new Game { Id = Guid.NewGuid(), Name = "Ruletka Nagród", Description = "Zakręć ruletką i zdobądź losową nagrodę!", Cost = 10, GameTypeId = lootType.Id });
        if (!await context.Games.AnyAsync(g => g.Name == "Ślepy Los") && cardsType != null)
            context.Games.Add(new Game { Id = Guid.NewGuid(), Name = "Ślepy Los", Description = "Wybierz jedną z trzech kart i sprawdź swoje szczęście.", Cost = 15, GameTypeId = cardsType.Id });
        if (!await context.Games.AnyAsync(g => g.Name == "Szczęśliwa Zdrapka") && scratchType != null)
            context.Games.Add(new Game { Id = Guid.NewGuid(), Name = "Szczęśliwa Zdrapka", Description = "Zdrap i odkryj ukrytą nagrodę.", Cost = 5, GameTypeId = scratchType.Id });

        await context.SaveChangesAsync();

        // 4. Przykładowe nagrody 
        var rarityLookup = await context.ItemRarities.ToDictionaryAsync(r => r.Name);

        var seedItems = new (string Name, string Rarity, int Price, string Description)[]
        {
            ("Odznaka Pierwszego Dnia",   "Pospolity",    10,  "Symboliczne potwierdzenie pierwszego dnia bez nawrotu."),
            ("Kompas Motywacji",           "Niepospolity", 20,  "Pomaga utrzymać kurs, gdy brakuje sił."),
            ("Srebrny Medal Wytrwałości",  "Rzadki",       35,  "Dla tych, którzy wytrwali tydzień bez potknięcia."),
            ("Kryształ Skupienia",         "Epicki",       55,  "Rzadka nagroda za miesiąc konsekwentnej pracy nad sobą."),
            ("Złota Korona Trzeźwości",    "Legendarny",   80,  "Legendarna oznaka – 90 dni bez nawrotu."),
            ("Mityczny Feniks Odrodzenia", "Mityczny",     100, "Najrzadsza nagroda: pół roku wytrwałości i pracy nad sobą.")
        };

        foreach (var item in seedItems)
        {
            if (!rarityLookup.TryGetValue(item.Rarity, out var rarity))
                continue;

            var existing = await context.RewardItems.FirstOrDefaultAsync(r => r.Name == item.Name);
            if (existing == null)
            {
                context.RewardItems.Add(new RewardItem
                {
                    Id = Guid.NewGuid(),
                    Name = item.Name,
                    Description = item.Description,
                    Price = item.Price,
                    IconPath = null,
                    ItemRarityId = rarity.Id
                });
            }
            else
            {
                // Uzupełniamy tylko brakujące Description / Price 
                if (string.IsNullOrWhiteSpace(existing.Description)) existing.Description = item.Description;
                if (existing.Price == 0)                              existing.Price       = item.Price;
            }
        }

        await context.SaveChangesAsync();

        // 5. Generyczny backfill 
        var stale = await context.RewardItems
            .Include(r => r.ItemRarity)
            .Where(r => r.Price == 0 || r.Description == null)
            .ToListAsync();

        foreach (var item in stale)
        {
            if (item.Price == 0)
            {
                item.Price = item.ItemRarity?.Name switch
                {
                    "Mityczny"     => 100,
                    "Legendarny"   => 80,
                    "Epicki"       => 55,
                    "Rzadki"       => 35,
                    "Niepospolity" => 20,
                    _              => 10
                };
            }

            if (string.IsNullOrWhiteSpace(item.Description))
                item.Description = $"Nagroda kategorii {item.ItemRarity?.Name ?? "Pospolity"}: {item.Name}.";
        }

        if (stale.Count > 0)
            await context.SaveChangesAsync();

        // 6. Jednorazowe sprzątanie po starej wersji seedera
        var legacyIcons = new[] { "/graphics/smile.png", "/graphics/pudzian.jpg" };
        var polluted = await context.RewardItems
            .Where(r => r.IconPath != null && legacyIcons.Contains(r.IconPath))
            .ToListAsync();

        foreach (var item in polluted)
            item.IconPath = null;

        if (polluted.Count > 0)
            await context.SaveChangesAsync();
        // 1. Słownik nastrojów
    if (!context.MoodTypes.Any())
    {
        context.MoodTypes.AddRange(
            new MoodType { Name = "Świetny" },
            new MoodType { Name = "Stabilny" },
            new MoodType { Name = "Lękowy" },
            new MoodType { Name = "Depresyjny" }
        );
        await context.SaveChangesAsync();
    }

    // 2. Statusy konsultacji
    if (!context.ConsultationStatuses.Any())
    {
        context.ConsultationStatuses.AddRange(
            new ConsultationStatus { Name = "Zaplanowana" },
            new ConsultationStatus { Name = "Zakończona" },
            new ConsultationStatus { Name = "Odwołana" }
        );
        await context.SaveChangesAsync();
    }

    // 3. Przykładowa ankieta
    if (!context.Surveys.Any())
    {
        var survey = new Survey { Id = Guid.NewGuid(), Title = "Ankieta samopoczucia po sesji" };
        context.Surveys.Add(survey);
        await context.SaveChangesAsync();

        context.SurveyQuestions.Add(new SurveyQuestion { 
            Id = Guid.NewGuid(), 
            SurveyId = survey.Id, 
            Text = "Jak oceniasz dzisiejsze postępy w skali 1-5?" 
        });
        await context.SaveChangesAsync();
    }


        // 1. Słownik nastrojów
        if (!context.MoodTypes.Any())
        {
            context.MoodTypes.AddRange(
                new MoodType { Name = "Świetny" },
                new MoodType { Name = "Stabilny" },
                new MoodType { Name = "Lękowy" },
                new MoodType { Name = "Depresyjny" }
            );
            await context.SaveChangesAsync();
        }

        // 2. Statusy konsultacji
        if (!context.ConsultationStatuses.Any())
        {
            context.ConsultationStatuses.AddRange(
                new ConsultationStatus { Name = "Zaplanowana" },
                new ConsultationStatus { Name = "Zakończona" },
                new ConsultationStatus { Name = "Odwołana" }
            );
            await context.SaveChangesAsync();
        }

        // 3. Przykładowa ankieta
        if (!context.Surveys.Any())
        {
            var survey = new Survey { Id = Guid.NewGuid(), Title = "Ankieta samopoczucia po sesji" };
            context.Surveys.Add(survey);
            await context.SaveChangesAsync();

            context.SurveyQuestions.Add(new SurveyQuestion
            {
                Id = Guid.NewGuid(),
                SurveyId = survey.Id,
                Text = "Jak oceniasz dzisiejsze postępy w skali 1-5?"
            });
            await context.SaveChangesAsync();
        }
        // 4. Pacjenci kroki
        if (!context.AchievementBadges.Any())
        {
            context.AchievementBadges.AddRange(
                new AchievementBadge
                {
                    Id = Guid.NewGuid(),
                    Name = "Pierwszy krok",
                    Description = "Ukończono pierwszy cel terapeutyczny.",
                    IconPath = "bi-star-fill",
                    ConditionType = "GoalsCompleted",
                    ConditionValue = 1
                },
                new AchievementBadge
                {
                    Id = Guid.NewGuid(),
                    Name = "Weteran",
                    Description = "Ukończono 5 celów terapeutycznych.",
                    IconPath = "bi-trophy-fill",
                    ConditionType = "GoalsCompleted",
                    ConditionValue = 5
                }
            );
            await context.SaveChangesAsync();
        }
    }
    
}