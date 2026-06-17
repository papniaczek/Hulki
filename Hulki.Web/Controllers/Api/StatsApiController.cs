using System.Threading.Tasks;
using Hulki.Web.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Hulki.Web.Controllers.Api;

/// <summary>
/// Wystawia statystyki użytkownika i systemu jako REST API.
/// Pod spodem każda metoda wywołuje procedurę składowaną lub funkcję SQL
/// (zobacz SqlObjectsService oraz migrację AddCustomUsersAndSqlObjects / AddMoreSqlObjects).
/// </summary>
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

    /// <summary>
    /// Zwraca pełne statystyki użytkownika: liczba zakończonych konsultacji,
    /// wpisów nastroju, raportów dziennych, zrealizowanych celów i saldo portfela.
    /// Wywołuje procedurę składowaną dbo.sp_GetUserStats.
    /// </summary>
    /// <param name="userId">Identyfikator użytkownika (AspNetUsers.Id)</param>
    [HttpGet("user/{userId}")]
    [ProducesResponseType(typeof(UserStatsDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<UserStatsDto>> GetUserStats(string userId)
    {
        var stats = await _sql.GetUserStatsAsync(userId);
        if (stats is null) return NotFound($"Nie znaleziono użytkownika o id '{userId}'.");
        return Ok(stats);
    }

    /// <summary>
    /// Zwraca sumę punktów zdobytych przez użytkownika (dodatnie transakcje portfela).
    /// Wywołuje funkcję dbo.fn_GetTotalEarnedPoints.
    /// </summary>
    /// <param name="userId">Identyfikator użytkownika (AspNetUsers.Id)</param>
    [HttpGet("user/{userId}/earned-points")]
    [ProducesResponseType(typeof(int), StatusCodes.Status200OK)]
    public async Task<ActionResult<int>> GetTotalEarnedPoints(string userId)
    {
        var total = await _sql.GetTotalEarnedPointsAsync(userId);
        return Ok(total);
    }

    /// <summary>
    /// Zwraca liczbę odznak zdobytych przez użytkownika.
    /// Wywołuje funkcję dbo.fn_CountUserBadges.
    /// </summary>
    /// <param name="userId">Identyfikator użytkownika (AspNetUsers.Id)</param>
    [HttpGet("user/{userId}/badge-count")]
    [ProducesResponseType(typeof(int), StatusCodes.Status200OK)]
    public async Task<ActionResult<int>> GetBadgeCount(string userId)
    {
        var count = await _sql.CountUserBadgesAsync(userId);
        return Ok(count);
    }

    /// <summary>
    /// Zwraca średnie saldo portfela wszystkich użytkowników w systemie.
    /// Wywołuje funkcję dbo.fn_AverageWalletBalance.
    /// </summary>
    [HttpGet("wallets/average-balance")]
    [ProducesResponseType(typeof(decimal), StatusCodes.Status200OK)]
    public async Task<ActionResult<decimal>> GetAverageWalletBalance()
    {
        var avg = await _sql.GetAverageWalletBalanceAsync();
        return Ok(avg);
    }
}