namespace Hulki.Web.Models.Dto;

public class QuoteDto
{
    public string Content { get; set; } = default!;
    public string? Author { get; set; }
    public string Source { get; set; } = default!;
}
