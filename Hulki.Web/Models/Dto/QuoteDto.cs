namespace Hulki.Web.Models.Dto;

/// <summary>
/// Ujednolicony cytat motywacyjny – niezależny od formatu źródła zewnętrznego.
/// </summary>
public class QuoteDto
{
    public string Content { get; set; } = default!;
    public string? Author { get; set; }
    public string Source { get; set; } = default!; // "ZenQuotes" albo "Fallback"
}
