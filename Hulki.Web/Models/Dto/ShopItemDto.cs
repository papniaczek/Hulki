namespace Hulki.Web.Models.Dto;

/// <summary>
/// DTO zwracane przez publiczne API sklepu (GET /api/shop/items).
/// Nie ujawnia wewnętrznych pól encji (np. FK rzadkości).
/// </summary>
public class ShopItemDto
{
    public string Id { get; set; } = default!;
    public string Name { get; set; } = default!;
    public string? Description { get; set; }
    public int Price { get; set; }
    public string? IconPath { get; set; }
    public string? Rarity { get; set; }
}
