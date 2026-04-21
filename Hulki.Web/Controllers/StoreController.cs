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

[Authorize] // Tylko zalogowani
    public class StoreController : Controller
    {
        private readonly ApplicationDbContext _context;
        private readonly UserManager<AppUser> _userManager;

        public StoreController(ApplicationDbContext context, UserManager<AppUser> userManager)
        {
            _context = context;
            _userManager = userManager;
        }

        // 1. WYŚWIETLENIE SKLEPU
        public async Task<IActionResult> Index()
        {
            await SeedStoreDataIfNotExists();

            var user = await _userManager.GetUserAsync(User);
            var wallet = await _context.Wallets.FirstOrDefaultAsync(w => w.AppUserId == user.Id);
            
            ViewBag.Points = wallet?.Balance ?? 0;

            var games = await _context.Games.Include(g => g.GameType).ToListAsync();
            return View(games);
        }

        // 2. OTWIERANIE SKRZYNKI
        [HttpPost]
        public async Task<IActionResult> OpenLootbox(Guid gameId)
        {
            var user = await _userManager.GetUserAsync(User);
            var wallet = await _context.Wallets.FirstOrDefaultAsync(w => w.AppUserId == user.Id);
            var game = await _context.Games.FirstOrDefaultAsync(g => g.Id == gameId);

            if (wallet == null || game == null || wallet.Balance < game.Cost)
            {
                TempData["ErrorMessage"] = "Nie masz wystarczająco punktów, aby otworzyć tę skrzynkę!";
                return RedirectToAction("Index");
            }

            wallet.Balance -= game.Cost;
            _context.PointTransactions.Add(new PointTransaction 
            { 
                Id = Guid.NewGuid(), Amount = -game.Cost, 
                Description = $"Otwarcie skrzynki: {game.Name}", 
                TransactionDate = DateTime.Now, WalletId = wallet.Id 
            });

            // Losowanie nagrody
            var allRewards = await _context.RewardItems.Include(r => r.ItemRarity).ToListAsync();
            var random = new Random();
            var wonItem = allRewards[random.Next(allRewards.Count)];

            // ZABEZPIECZENIE PRZED DUPLIKATEM
            bool alreadyOwns = await _context.PatientInventories.AnyAsync(pi => pi.AppUserId == user.Id && pi.RewardItemId == wonItem.Id);
            
            if (alreadyOwns)
            {
                int refund = game.Cost / 2;
                wallet.Balance += refund;
                _context.PointTransactions.Add(new PointTransaction { Id = Guid.NewGuid(), Amount = refund, Description = $"Duplikat: {wonItem.Name} (Zwrot pkt)", TransactionDate = DateTime.Now, WalletId = wallet.Id });
                TempData["SuccessMessage"] = $"Wylosowano: {wonItem.Name} ({wonItem.ItemRarity.Name}), ale już to masz w ekwipunku! Otrzymujesz {refund} pkt zwrotu.";
            }
            else
            {
                _context.PatientInventories.Add(new PatientInventory { AppUserId = user.Id, RewardItemId = wonItem.Id });
                TempData["SuccessMessage"] = $"Gratulacje! Wygrałeś nowy przedmiot: {wonItem.Name} [{wonItem.ItemRarity.Name}]!";
            }

            _context.GameSessions.Add(new GameSession { Id = Guid.NewGuid(), AppUserId = user.Id, GameId = game.Id, PlayedAt = DateTime.Now });

            await _context.SaveChangesAsync();
            return RedirectToAction("Index");
        }

        private async Task SeedStoreDataIfNotExists()
        {
            if (!await _context.ItemRarities.AnyAsync())
            {
                _context.ItemRarities.AddRange(
                    new ItemRarity { Name = "Zwykły", HexColor = "#808080" },
                    new ItemRarity { Name = "Legendarny", HexColor = "#FFA500" }
                );
                await _context.SaveChangesAsync();
            }

            if (!await _context.GameTypes.AnyAsync())
            {
                _context.GameTypes.Add(new GameType { Name = "Lootbox", Description = "Skrzynia z losową zawartością" });
                await _context.SaveChangesAsync();
            }

            if (!await _context.RewardItems.AnyAsync())
            {
                var normal = await _context.ItemRarities.FirstOrDefaultAsync(r => r.Name == "Zwykły");
                var legendary = await _context.ItemRarities.FirstOrDefaultAsync(r => r.Name == "Legendarny");
                _context.RewardItems.AddRange(
                    new RewardItem { Id = Guid.NewGuid(), Name = "Miedziana Odznaka Startu", ItemRarityId = normal.Id },
                    new RewardItem { Id = Guid.NewGuid(), Name = "Tytuł: Wojownik Światła", ItemRarityId = normal.Id },
                    new RewardItem { Id = Guid.NewGuid(), Name = "Legendarny Puchar Niezłomnych", ItemRarityId = legendary.Id }
                );
                await _context.SaveChangesAsync();
            }

            if (!await _context.Games.AnyAsync())
            {
                var lootboxType = await _context.GameTypes.FirstOrDefaultAsync(t => t.Name == "Lootbox");
                _context.Games.Add(new Game 
                { 
                    Id = Guid.NewGuid(), 
                    Name = "Skrzynia Motywacji", 
                    Description = "Wydaj 10 punktów i spróbuj zdobyć legendarne wyposażenie do swojego profilu!", 
                    Cost = 10, 
                    GameTypeId = lootboxType.Id 
                });
                await _context.SaveChangesAsync();
            }
        }
    }