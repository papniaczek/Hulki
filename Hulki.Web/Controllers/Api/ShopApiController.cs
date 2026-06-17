using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Hulki.Web.Models.Dto;
using Hulki.Web.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Hulki.Web.Controllers.Api;

/// <summary>
/// Wystawia listę przedmiotów dostępnych w sklepie nagród (RewardItems).
/// </summary>
[ApiController]
[Route("api/shop")]
[AllowAnonymous]
[Produces("application/json")]
public class ShopApiController : ControllerBase
{
    private readonly IShopService _shopService;

    public ShopApiController(IShopService shopService)
    {
        _shopService = shopService;
    }

    /// <summary>Zwraca wszystkie przedmioty dostępne w sklepie nagród.</summary>
    [HttpGet("items")]
    [ProducesResponseType(typeof(IReadOnlyList<ShopItemDto>), StatusCodes.Status200OK)]
    public async Task<ActionResult<IReadOnlyList<ShopItemDto>>> GetItems(
        CancellationToken cancellationToken
    )
    {
        var items = await _shopService.GetShopItemsAsync(cancellationToken);
        return Ok(items);
    }
}