using System.Threading;
using System.Threading.Tasks;
using Hulki.Web.Models.Dto;
using Hulki.Web.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Hulki.Web.Controllers.Api;

// losowe cytaty motywacyjne (zenquotes.io)
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

    // zwraca cytat
    [HttpGet("random")]
    [ProducesResponseType(typeof(QuoteDto), StatusCodes.Status200OK)]
    public async Task<ActionResult<QuoteDto>> GetRandom(CancellationToken cancellationToken)
    {
        var quote = await _quoteService.GetRandomQuoteAsync(forceRefresh: true, cancellationToken);
        return Ok(quote);
    }
}