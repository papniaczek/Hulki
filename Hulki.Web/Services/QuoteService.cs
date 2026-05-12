using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Net.Http.Json;
using System.Text.Json.Serialization;
using System.Threading;
using System.Threading.Tasks;
using Hulki.Web.Models.Dto;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Logging;

namespace Hulki.Web.Services;

public class QuoteService : IQuoteService
{
    public const string QuotesClientName      = "ZenQuotes";
    public const string TranslationClientName = "MyMemory";
    private const string CacheKey = "quote:random";
    private static readonly TimeSpan CacheDuration = TimeSpan.FromMinutes(10);

    private readonly IHttpClientFactory _httpClientFactory;
    private readonly IMemoryCache _cache;
    private readonly ILogger<QuoteService> _logger;

    public QuoteService(IHttpClientFactory httpClientFactory, IMemoryCache cache, ILogger<QuoteService> logger)
    {
        _httpClientFactory = httpClientFactory;
        _cache = cache;
        _logger = logger;
    }

    public async Task<QuoteDto> GetRandomQuoteAsync(bool forceRefresh = false, CancellationToken cancellationToken = default)
    {
        // Cache używamy tylko przy renderze strony (SSR). Przycisk "Nowy cytat" wywołuje
        // serwis z forceRefresh=true, żeby faktycznie dostać coś nowego.
        if (!forceRefresh && _cache.TryGetValue<QuoteDto>(CacheKey, out var cached) && cached is not null)
            return cached;

        QuoteDto quote;
        try
        {
            quote = await FetchFromZenQuotesAsync(cancellationToken) ?? PickFallback();
        }
        catch (Exception ex)
        {
            _logger.LogWarning(ex, "Nie udało się pobrać cytatu z zewnętrznego API, używam fallbacku.");
            quote = PickFallback();
        }

        _cache.Set(CacheKey, quote, CacheDuration);
        return quote;
    }

    private async Task<QuoteDto?> FetchFromZenQuotesAsync(CancellationToken cancellationToken)
    {
        var client = _httpClientFactory.CreateClient(QuotesClientName);

        // ZenQuotes zwraca tablicę z jednym elementem dla /random.
        var items = await client.GetFromJsonAsync<ZenQuoteItem[]>("random", cancellationToken);

        if (items is null || items.Length == 0 || string.IsNullOrWhiteSpace(items[0].Quote))
            return null;

        var first = items[0];
        var englishQuote = first.Quote!.Trim();
        var author = string.IsNullOrWhiteSpace(first.Author) ? null : first.Author!.Trim();

        // ZenQuotes oddaje tylko po angielsku, więc tłumaczymy treść cytatu na polski.
        // Nazwisko autora zostawiamy w oryginale (imiona własne nie powinny być tłumaczone).
        var translated = await TranslateToPolishAsync(englishQuote, cancellationToken);
        var content = translated ?? englishQuote;

        return new QuoteDto
        {
            Content = content,
            Author = author,
            Source = translated != null ? "ZenQuotes + MyMemory (PL)" : "ZenQuotes (EN)"
        };
    }

    private async Task<string?> TranslateToPolishAsync(string text, CancellationToken cancellationToken)
    {
        try
        {
            var client = _httpClientFactory.CreateClient(TranslationClientName);
            var url = $"get?q={Uri.EscapeDataString(text)}&langpair=en|pl";

            var result = await client.GetFromJsonAsync<MyMemoryResponse>(url, cancellationToken);
            var translated = result?.ResponseData?.TranslatedText?.Trim();

            if (string.IsNullOrWhiteSpace(translated))
                return null;

            // MyMemory przy błędzie potrafi zwrócić komunikat typu "PLEASE SELECT TWO DISTINCT LANGUAGES"
            // albo "MYMEMORY WARNING: ..." – wtedy lepiej zostawić oryginał.
            if (translated.StartsWith("PLEASE ", StringComparison.OrdinalIgnoreCase) ||
                translated.StartsWith("MYMEMORY", StringComparison.OrdinalIgnoreCase))
                return null;

            return translated;
        }
        catch (Exception ex)
        {
            _logger.LogDebug(ex, "Tłumaczenie cytatu na PL nie powiodło się, zostawiam oryginał.");
            return null;
        }
    }

    private static readonly (string Content, string Author)[] FallbackQuotes =
    {
        ("Każdy dzień to nowa szansa, by być lepszym niż wczoraj.", "Nieznany"),
        ("Nie musisz być świetny, żeby zacząć. Musisz zacząć, żeby być świetny.", "Zig Ziglar"),
        ("Siła nie bierze się z wygrywania. Bierze się z walki.", "Arnold Schwarzenegger"),
        ("Cierpliwość i wytrwałość pokonują wszystko.", "Ralph Waldo Emerson"),
        ("Najciemniej jest tuż przed świtem.", "Thomas Fuller"),
        ("Małe kroki każdego dnia prowadzą do wielkich zmian.", "Nieznany"),
        ("Nie liczy się to, jak często upadasz, lecz jak często wstajesz.", "Vince Lombardi"),
        ("Najlepszy czas, by zacząć, był wczoraj. Drugi najlepszy – dziś.", "Przysłowie chińskie")
    };

    private static int _lastFallbackIndex = -1;

    private static QuoteDto PickFallback()
    {
        // Losujemy tak, by nie trafić dwa razy z rzędu w ten sam cytat – inaczej przy częstym
        // klikaniu "Nowy cytat" UI wyglądałby jakby przycisk nic nie robił.
        int idx;
        if (FallbackQuotes.Length == 1)
        {
            idx = 0;
        }
        else
        {
            do { idx = Random.Shared.Next(FallbackQuotes.Length); }
            while (idx == _lastFallbackIndex);
        }
        _lastFallbackIndex = idx;

        var pick = FallbackQuotes[idx];
        return new QuoteDto { Content = pick.Content, Author = pick.Author, Source = "Fallback" };
    }

    // DTO odpowiadające kształtowi odpowiedzi ZenQuotes: [{ "q": "...", "a": "...", "h": "..." }]
    private sealed class ZenQuoteItem
    {
        [JsonPropertyName("q")] public string? Quote { get; set; }
        [JsonPropertyName("a")] public string? Author { get; set; }
    }

    // DTO odpowiadające odpowiedzi MyMemory: { "responseData": { "translatedText": "..." }, ... }
    private sealed class MyMemoryResponse
    {
        [JsonPropertyName("responseData")] public MyMemoryData? ResponseData { get; set; }
    }

    private sealed class MyMemoryData
    {
        [JsonPropertyName("translatedText")] public string? TranslatedText { get; set; }
    }
}
