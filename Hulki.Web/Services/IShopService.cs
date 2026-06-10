using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Hulki.Web.Models.Dto;

namespace Hulki.Web.Services;

public interface IShopService
{
    Task<IReadOnlyList<ShopItemDto>> GetShopItemsAsync(
        CancellationToken cancellationToken = default
    );
}
