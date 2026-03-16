using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Hulki.Web.Models;

public class RewardItem
{
    [Key]
    public Guid Id { get; set; }

    [Required]
    public string Name { get; set; } // np. "Odznaka Żelaznej Woli"

    // Rzadkość ze słownika (Pospolity, Legendarny itp.)
    public int ItemRarityId { get; set; }
    [ForeignKey("ItemRarityId")]
    public virtual ItemRarity ItemRarity { get; set; }
}