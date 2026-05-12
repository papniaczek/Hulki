using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Hulki.Web.Models;

public class RewardItem
{
    [Key]
    public Guid Id { get; set; }

    [Required]
    [MaxLength(150)]
    public string Name { get; set; } // np. "Odznaka Żelaznej Woli"

    [MaxLength(1000)]
    public string? Description { get; set; }

    // Cena sklepowa przedmiotu w punktach (używana przez API sklepu).
    public int Price { get; set; }

    // Ścieżka do miniatury / ikony (np. "/images/shop/badge.png").
    [MaxLength(300)]
    public string? IconPath { get; set; }

    // Rzadkość ze słownika (Pospolity, Legendarny itp.)
    public int ItemRarityId { get; set; }
    [ForeignKey("ItemRarityId")]
    public virtual ItemRarity ItemRarity { get; set; }
}
