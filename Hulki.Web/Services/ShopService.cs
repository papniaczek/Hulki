using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Hulki.Web.Data;
using Hulki.Web.Models.Dto;
using Microsoft.EntityFrameworkCore;

namespace Hulki.Web.Services;

public class ShopService : IShopService
{
    private readonly ApplicationDbContext _context;

    public ShopService(ApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<IReadOnlyList<ShopItemDto>> GetShopItemsAsync(CancellationToken cancellationToken = default)
    {
        // Pobieranie danych prosto z bazy przez EF Core i mapowanie do DTO po stronie SQL.
        var items = await _context.RewardItems
            .AsNoTracking()
            .Include(r => r.ItemRarity)
            .OrderBy(r => r.Price)
            .ThenBy(r => r.Name)
            .Select(r => new ShopItemDto
            {
                Id = r.Id.ToString(),
                Name = r.Name,
                Description = r.Description,
                Price = r.Price,
                IconPath = r.IconPath,
                Rarity = r.ItemRarity != null ? r.ItemRarity.Name : null
            })
            .ToListAsync(cancellationToken);

        return items;
    }
}
