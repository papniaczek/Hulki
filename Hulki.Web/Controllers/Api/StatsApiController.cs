using System.Threading.Tasks;
using Hulki.Web.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Hulki.Web.Controllers.Api;

// wystawia statystyki uzytkownika
[ApiController]
[Route("api/stats")]
[AllowAnonymous]
[Produces("application/json")]
public class StatsApiController : ControllerBase
{
    private readonly SqlObjectsService _sql;

    public StatsApiController(SqlObjectsService sql)
    {
        _sql = sql;
    }

    // pelne statystyki
    [HttpGet("user/{userId}")]
    [ProducesResponseType(typeof(UserStatsDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<UserStatsDto>> GetUserStats(string userId)
    {
        var stats = await _sql.GetUserStatsAsync(userId);
        if (stats is null) return NotFound($"Nie znaleziono użytkownika o id '{userId}'.");
        return Ok(stats);
    }

    // suma punktow uzytkownika
    /// <param name="userId">Identyfikator użytkownika (AspNetUsers.Id)</param>
    [HttpGet("user/{userId}/earned-points")]
    [ProducesResponseType(typeof(int), StatusCodes.Status200OK)]
    public async Task<ActionResult<int>> GetTotalEarnedPoints(string userId)
    {
        var total = await _sql.GetTotalEarnedPointsAsync(userId);
        return Ok(total);
    }

    // liczba odznak uzytkownika
    /// <param name="userId">Identyfikator użytkownika (AspNetUsers.Id)</param>
    [HttpGet("user/{userId}/badge-count")]
    [ProducesResponseType(typeof(int), StatusCodes.Status200OK)]
    public async Task<ActionResult<int>> GetBadgeCount(string userId)
    {
        var count = await _sql.CountUserBadgesAsync(userId);
        return Ok(count);
    }

    // srednie saldo wszystkich portfeli
    [HttpGet("wallets/average-balance")]
    [ProducesResponseType(typeof(decimal), StatusCodes.Status200OK)]
    public async Task<ActionResult<decimal>> GetAverageWalletBalance()
    {
        var avg = await _sql.GetAverageWalletBalanceAsync();
        return Ok(avg);
    }
}