using System.Threading;
using System.Threading.Tasks;
using Hulki.Web.Models.Dto;

namespace Hulki.Web.Services;

public interface IQuoteService
{
    /// <summary>
    /// </summary>
    /// <param name="forceRefresh">Gdy true, pomija lokalny cache i pobiera świeży cytat.</param>
    Task<QuoteDto> GetRandomQuoteAsync(bool forceRefresh = false, CancellationToken cancellationToken = default);
}
