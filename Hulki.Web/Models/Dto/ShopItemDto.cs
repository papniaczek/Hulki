namespace Hulki.Web.Models.Dto;

public class ShopItemDto
{
    public string Id { get; set; } = default!;
    public string Name { get; set; } = default!;
    public string? Description { get; set; }
    public int Price { get; set; }
    public string? IconPath { get; set; }
    public string? Rarity { get; set; }
}
