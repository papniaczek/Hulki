using System.Threading;
using System.Threading.Tasks;
using Hulki.Web.Models.Dto;
using Hulki.Web.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Hulki.Web.Controllers.Api;

/// <summary>
/// Lekki endpoint do pobierania pojedynczego cytatu (używany przez frontend,
/// np. przycisk "Nowy cytat" na stronie głównej).
/// </summary>
[ApiController]
[Route("api/quotes")]
[AllowAnonymous]
[Produces("application/json")]
public class QuotesApiController : ControllerBase
{
    private readonly IQuoteService _quoteService;

    public QuotesApiController(IQuoteService quoteService)
    {
        _quoteService = quoteService;
    }

    [HttpGet("random")]
    [ProducesResponseType(typeof(QuoteDto), StatusCodes.Status200OK)]
    public async Task<ActionResult<QuoteDto>> GetRandom(CancellationToken cancellationToken)
    {
        // Endpoint wołany głównie z przycisku "Nowy cytat" – omijamy cache, żeby
        // faktycznie dostać świeżą treść z zewnętrznego API (lub fallbacku).
        var quote = await _quoteService.GetRandomQuoteAsync(forceRefresh: true, cancellationToken);
        return Ok(quote);
    }
}
