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
        await StoreDataSeeder.SeedAsync(_context);

        var user = await _userManager.GetUserAsync(User);
        var wallet = await _context.Wallets.FirstOrDefaultAsync(w => w.AppUserId == user.Id);

        ViewBag.Points = wallet?.Balance ?? 0;
        ViewBag.Rarities = await _context.ItemRarities.ToListAsync();

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
            TempData["ErrorMessage"] = "Nie masz wystarczająco punktów, aby zagrać!";
            return RedirectToAction("Index");
        }

        var allRewards = await _context.RewardItems.Include(r => r.ItemRarity).ToListAsync();
        if (!allRewards.Any())
        {
            TempData["ErrorMessage"] = "Brak nagród w bazie!";
            return RedirectToAction("Index");
        }

        wallet.Balance -= game.Cost;
        _context.PointTransactions.Add(new PointTransaction { Id = Guid.NewGuid(), Amount = -game.Cost, Description = $"Gra: {game.Name}", TransactionDate = DateTime.Now, WalletId = wallet.Id });

        var random = new Random();
        var wonItem = allRewards[random.Next(allRewards.Count)];

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

        ViewBag.WonItem = wonItem;
        ViewBag.AllRewards = allRewards.Select(r => new { name = r.Name, rarity = r.ItemRarity.Name }).ToList();

        return View("RouletteGame");
    }

    [HttpPost]
    public async Task<IActionResult> PlayCards(Guid gameId)
    {
        var user = await _userManager.GetUserAsync(User);
        var wallet = await _context.Wallets.FirstOrDefaultAsync(w => w.AppUserId == user.Id);
        var game = await _context.Games.FirstOrDefaultAsync(g => g.Id == gameId);

        if (wallet == null || game == null || wallet.Balance < game.Cost)
        {
            TempData["ErrorMessage"] = "Nie masz wystarczająco punktów, aby zagrać!";
            return RedirectToAction("Index");
        }

        var allRewards = await _context.RewardItems.Include(r => r.ItemRarity).ToListAsync();
        if (allRewards.Count < 3)
        {
            TempData["ErrorMessage"] = "Za mało nagród w bazie do tej gry!";
            return RedirectToAction("Index");
        }

        wallet.Balance -= game.Cost;
        _context.PointTransactions.Add(new PointTransaction { Id = Guid.NewGuid(), Amount = -game.Cost, Description = $"Gra: {game.Name}", TransactionDate = DateTime.Now, WalletId = wallet.Id });

        var random = new Random();
        var shuffled = allRewards.OrderBy(x => random.Next()).Take(3).ToList();
        var wonItem = shuffled[0];

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

        ViewBag.WonItem = wonItem;
        ViewBag.Missed1 = shuffled[1];
        ViewBag.Missed2 = shuffled[2];

        return View("CardsGame");
    }

    [HttpPost]
    public async Task<IActionResult> PlayScratchcard(Guid gameId)
    {
        var user = await _userManager.GetUserAsync(User);
        var wallet = await _context.Wallets.FirstOrDefaultAsync(w => w.AppUserId == user.Id);
        var game = await _context.Games.FirstOrDefaultAsync(g => g.Id == gameId);

        if (wallet == null || game == null || wallet.Balance < game.Cost)
        {
            TempData["ErrorMessage"] = "Nie masz wystarczająco punktów, aby zagrać!";
            return RedirectToAction("Index");
        }

        var allRewards = await _context.RewardItems.Include(r => r.ItemRarity).ToListAsync();
        if (!allRewards.Any()) return RedirectToAction("Index");

        wallet.Balance -= game.Cost;
        _context.PointTransactions.Add(new PointTransaction { Id = Guid.NewGuid(), Amount = -game.Cost, Description = $"Gra: {game.Name}", TransactionDate = DateTime.Now, WalletId = wallet.Id });

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

    [HttpPost]
    public async Task<IActionResult> SellItem(Guid itemId)
    {
        var user = await _userManager.GetUserAsync(User);
        var inventoryEntry = await _context.PatientInventories
            .Include(pi => pi.RewardItem)
            .ThenInclude(ri => ri.ItemRarity)
            .FirstOrDefaultAsync(pi => pi.AppUserId == user.Id && pi.RewardItemId == itemId);

        if (inventoryEntry == null)
        {
            TempData["ErrorMessage"] = "Nie znaleziono przedmiotu w ekwipunku.";
            return RedirectToAction("Index", "Profile");
        }

        int sellPrice = inventoryEntry.RewardItem.ItemRarity?.Name switch
        {
            "Mityczny" => 40,
            "Legendarny" => 25,
            "Epicki" => 15,
            "Rzadki" => 10,
            "Niepospolity" => 6,
            _ => 3
        };

        _context.PatientInventories.Remove(inventoryEntry);

        var wallet = await _context.Wallets.FirstOrDefaultAsync(w => w.AppUserId == user.Id);
        if (wallet == null)
        {
            wallet = new Wallet { Id = Guid.NewGuid(), AppUserId = user.Id, Balance = 0 };
            _context.Wallets.Add(wallet);
        }
        wallet.Balance += sellPrice;

        _context.PointTransactions.Add(new PointTransaction
        {
            Id = Guid.NewGuid(),
            Amount = sellPrice,
            Description = $"Sprzedaż: {inventoryEntry.RewardItem.Name}",
            TransactionDate = DateTime.Now,
            WalletId = wallet.Id
        });

        await _context.SaveChangesAsync();
        TempData["SuccessMessage"] = $"Sprzedano {inventoryEntry.RewardItem.Name} za {sellPrice} pkt!";
        return RedirectToAction("Index", "Profile");
    }


    [HttpPost]
    public async Task<IActionResult> SellMultiple(List<Guid> itemIds)
    {
        if (itemIds == null || !itemIds.Any())
        {
            TempData["ErrorMessage"] = "Nie wybrano zadnych przedmiotow.";
            return RedirectToAction("Index", "Profile");
        }

        var user = await _userManager.GetUserAsync(User);
        var entries = await _context.PatientInventories
            .Include(pi => pi.RewardItem)
            .ThenInclude(ri => ri.ItemRarity)
            .Where(pi => pi.AppUserId == user.Id && itemIds.Contains(pi.RewardItemId))
            .ToListAsync();

        if (!entries.Any())
        {
            TempData["ErrorMessage"] = "Nie znaleziono wybranych przedmiotow.";
            return RedirectToAction("Index", "Profile");
        }

        var wallet = await _context.Wallets.FirstOrDefaultAsync(w => w.AppUserId == user.Id);
        if (wallet == null)
        {
            wallet = new Wallet { Id = Guid.NewGuid(), AppUserId = user.Id, Balance = 0 };
            _context.Wallets.Add(wallet);
        }

        int totalPrice = 0;
        foreach (var entry in entries)
        {
            int sellPrice = entry.RewardItem.ItemRarity?.Name switch
            {
                "Mityczny" => 40,
                "Legendarny" => 25,
                "Epicki" => 15,
                "Rzadki" => 10,
                "Niepospolity" => 6,
                _ => 3
            };
            totalPrice += sellPrice;

            _context.PointTransactions.Add(new PointTransaction
            {
                Id = Guid.NewGuid(),
                Amount = sellPrice,
                Description = $"Sprzedaz: {entry.RewardItem.Name}",
                TransactionDate = DateTime.Now,
                WalletId = wallet.Id
            });

            _context.PatientInventories.Remove(entry);
        }

        wallet.Balance += totalPrice;
        await _context.SaveChangesAsync();

        TempData["SuccessMessage"] = $"Sprzedano {entries.Count} przedmiotow za lacznie {totalPrice} pkt!";
        return RedirectToAction("Index", "Profile");
    }

    [HttpPost]
    public async Task<IActionResult> AddRewardItem(string name, int itemRarityId, string? description, int price, string? iconPath)
    {
        if (!string.IsNullOrWhiteSpace(name))
        {
            _context.RewardItems.Add(new RewardItem
            {
                Id = Guid.NewGuid(),
                Name = name,
                Description = string.IsNullOrWhiteSpace(description) ? null : description,
                Price = price,
                IconPath = string.IsNullOrWhiteSpace(iconPath) ? null : iconPath,
                ItemRarityId = itemRarityId
            });
            await _context.SaveChangesAsync();
            TempData["SuccessMessage"] = $"Dodano przedmiot: {name}!";
        }
        return RedirectToAction("Index");
    }

    private async Task SeedStoreDataIfNotExists()
        => await StoreDataSeeder.SeedAsync(_context);
}