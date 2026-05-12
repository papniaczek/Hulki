using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Hulki.Web.Models.Dto;

namespace Hulki.Web.Services;

public interface IShopService
{
    /// <summary>
    /// Zwraca aktualną ofertę przedmiotów dostępnych w sklepie.
    /// </summary>
    Task<IReadOnlyList<ShopItemDto>> GetShopItemsAsync(CancellationToken cancellationToken = default);
}
