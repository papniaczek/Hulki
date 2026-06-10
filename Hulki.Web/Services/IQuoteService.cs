using System.Threading;
using System.Threading.Tasks;
using Hulki.Web.Models.Dto;

namespace Hulki.Web.Services;

public interface IQuoteService
{
    Task<QuoteDto> GetRandomQuoteAsync(
        bool forceRefresh = false,
        CancellationToken cancellationToken = default
    );
}
