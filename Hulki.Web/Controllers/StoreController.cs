using Hulki.Web.Data;
using Hulki.Web.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System;
using System.Linq;
using System.Threading.Tasks;
using System.Collections.Generic;

namespace Hulki.Web.Controllers;

[Authorize]
public class StoreController : Controller
{
    private readonly ApplicationDbContext _context;
    private readonly UserManager<AppUser> _userManager;

    public StoreController(ApplicationDbContext context, UserManager<AppUser> userManager)
    {
        _context = context;
        _userManager = userManager;
    }

    public async Task<IActionResult> Index()
    {
        await SeedStoreDataIfNotExists();

        var user = await _userManager.GetUserAsync(User);
        var wallet = await _context.Wallets.FirstOrDefaultAsync(w => w.AppUserId == user.Id);
        
        ViewBag.Points = wallet?.Balance ?? 0;

        var games = await _context.Games.Include(g => g.GameType).ToListAsync();
        return View(games);
    }

    [HttpPost]
    public async Task<IActionResult> PlayRoulette(Guid gameId)
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
        _context.PointTransactions.Add(new PointTransaction { Id = Guid.NewGuid(), Amount = -game.Cost, Description = $"Gra: {game.Name}", TransactionDate = DateTime.Now, WalletId = wallet.Id });

        // Losowanie nagrody
        var allRewards = await _context.RewardItems.Include(r => r.ItemRarity).ToListAsync();
        var random = new Random();
        var wonItem = allRewards[random.Next(allRewards.Count)];

        // Zabezpieczenie przed duplikatem
        bool alreadyOwns = await _context.PatientInventories.AnyAsync(pi => pi.AppUserId == user.Id && pi.RewardItemId == wonItem.Id);
        if (alreadyOwns)
        {
            int refund = game.Cost / 2;
            wallet.Balance += refund;
            _context.PointTransactions.Add(new PointTransaction { Id = Guid.NewGuid(), Amount = refund, Description = $"Duplikat: {wonItem.Name} (Zwrot pkt)", TransactionDate = DateTime.Now, WalletId = wallet.Id });
            ViewBag.Message = $"Wylosowano duplikat! Otrzymujesz {refund} pkt zwrotu.";
        }
        else
        {
            _context.PatientInventories.Add(new PatientInventory { AppUserId = user.Id, RewardItemId = wonItem.Id });
            ViewBag.Message = $"Gratulacje! Wygrałeś nowy przedmiot: {wonItem.Name}!";
        }

        _context.GameSessions.Add(new GameSession { Id = Guid.NewGuid(), AppUserId = user.Id, GameId = game.Id, PlayedAt = DateTime.Now });
        await _context.SaveChangesAsync();

        // Przekazanie danych do widoku Ruletki
        ViewBag.WonItem = wonItem;
        
        // Przekazujemy uproszczoną listę wszystkich nagród do wygenerowania ruletki
        ViewBag.AllRewards = allRewards.Select(r => new { 
            name = r.Name, 
            rarity = r.ItemRarity.Name 
        }).ToList();

        return View("RouletteGame");
    }

    // --- NOWA GRA: 3 KARTY ---
    [HttpPost]
    public async Task<IActionResult> PlayCards(Guid gameId)
    {
        var user = await _userManager.GetUserAsync(User);
        var wallet = await _context.Wallets.FirstOrDefaultAsync(w => w.AppUserId == user.Id);
        var game = await _context.Games.FirstOrDefaultAsync(g => g.Id == gameId);

        if (wallet == null || game == null || wallet.Balance < game.Cost)
        {
            TempData["ErrorMessage"] = "Nie masz wystarczająco punktów!";
            return RedirectToAction("Index");
        }

        // Płatność
        wallet.Balance -= game.Cost;
        _context.PointTransactions.Add(new PointTransaction { Id = Guid.NewGuid(), Amount = -game.Cost, Description = $"Gra: {game.Name}", TransactionDate = DateTime.Now, WalletId = wallet.Id });

        // Losowanie 3 różnych przedmiotów
        var allRewards = await _context.RewardItems.Include(r => r.ItemRarity).ToListAsync();
        var random = new Random();
        var shuffled = allRewards.OrderBy(x => random.Next()).Take(3).ToList();
        
        var wonItem = shuffled[0]; // To gracz "wygra"
        var missed1 = shuffled[1]; // To pokaże się na innych kartach
        var missed2 = shuffled[2];

        // Zabezpieczenie przed duplikatem
        bool alreadyOwns = await _context.PatientInventories.AnyAsync(pi => pi.AppUserId == user.Id && pi.RewardItemId == wonItem.Id);
        if (alreadyOwns)
        {
            int refund = game.Cost / 2;
            wallet.Balance += refund;
            _context.PointTransactions.Add(new PointTransaction { Id = Guid.NewGuid(), Amount = refund, Description = $"Duplikat (Zwrot)", TransactionDate = DateTime.Now, WalletId = wallet.Id });
            ViewBag.Message = $"Wylosowano duplikat! Otrzymujesz {refund} pkt zwrotu.";
        }
        else
        {
            _context.PatientInventories.Add(new PatientInventory { AppUserId = user.Id, RewardItemId = wonItem.Id });
            ViewBag.Message = $"Gratulacje! Wygrałeś: {wonItem.Name}!";
        }

        _context.GameSessions.Add(new GameSession { Id = Guid.NewGuid(), AppUserId = user.Id, GameId = game.Id, PlayedAt = DateTime.Now });
        await _context.SaveChangesAsync();

        // Przekazanie danych do widoku "3 Kart"
        ViewBag.WonItem = wonItem;
        ViewBag.Missed1 = missed1;
        ViewBag.Missed2 = missed2;

        return View("CardsGame"); 
    }

    // --- NOWA GRA: ZDRAPKA ---
    [HttpPost]
    public async Task<IActionResult> PlayScratchcard(Guid gameId)
    {
        var user = await _userManager.GetUserAsync(User);
        var wallet = await _context.Wallets.FirstOrDefaultAsync(w => w.AppUserId == user.Id);
        var game = await _context.Games.FirstOrDefaultAsync(g => g.Id == gameId);

        if (wallet == null || game == null || wallet.Balance < game.Cost)
        {
            TempData["ErrorMessage"] = "Nie masz wystarczająco punktów!";
            return RedirectToAction("Index");
        }

        wallet.Balance -= game.Cost;
        _context.PointTransactions.Add(new PointTransaction { Id = Guid.NewGuid(), Amount = -game.Cost, Description = $"Gra: {game.Name}", TransactionDate = DateTime.Now, WalletId = wallet.Id });

        var allRewards = await _context.RewardItems.Include(r => r.ItemRarity).ToListAsync();
        var random = new Random();
        var wonItem = allRewards[random.Next(allRewards.Count)];

        bool alreadyOwns = await _context.PatientInventories.AnyAsync(pi => pi.AppUserId == user.Id && pi.RewardItemId == wonItem.Id);
        if (alreadyOwns)
        {
            wallet.Balance += (game.Cost / 2);
            ViewBag.Message = $"Duplikat. Zwrot {game.Cost / 2} pkt.";
        }
        else
        {
            _context.PatientInventories.Add(new PatientInventory { AppUserId = user.Id, RewardItemId = wonItem.Id });
            ViewBag.Message = $"Nowy przedmiot w ekwipunku!";
        }

        _context.GameSessions.Add(new GameSession { Id = Guid.NewGuid(), AppUserId = user.Id, GameId = game.Id, PlayedAt = DateTime.Now });
        await _context.SaveChangesAsync();

        ViewBag.WonItem = wonItem;
        return View("ScratchcardGame");
    }

    private async Task SeedStoreDataIfNotExists()
    {
        // ... (Tu zostaje Twój kod seedowania rzadkości i nagród) ...

        if (!await _context.GameTypes.AnyAsync(t => t.Name == "3 Karty"))
        {
            _context.GameTypes.Add(new GameType { Name = "3 Karty", Description = "Wybierz jedną z 3 kart" });
            _context.GameTypes.Add(new GameType { Name = "Zdrapka", Description = "Odkryj pole, aby sprawdzić nagrodę" });
            await _context.SaveChangesAsync();
        }

        if (!await _context.Games.AnyAsync(g => g.Name == "Ślepy Los"))
        {
            var cardsType = await _context.GameTypes.FirstOrDefaultAsync(t => t.Name == "3 Karty");
            var scratchType = await _context.GameTypes.FirstOrDefaultAsync(t => t.Name == "Zdrapka");

            _context.Games.Add(new Game { Id = Guid.NewGuid(), Name = "Ślepy Los", Description = "Wybierz 1 z 3 kart. Co kryje reszta?", Cost = 15, GameTypeId = cardsType.Id });
            _context.Games.Add(new Game { Id = Guid.NewGuid(), Name = "Szczęśliwa Zdrapka", Description = "Kup i zdrap pole!", Cost = 5, GameTypeId = scratchType.Id });
            await _context.SaveChangesAsync();
        }
    }
}