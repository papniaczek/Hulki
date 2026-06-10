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
    public string Name { get; set; }

    [MaxLength(1000)]
    public string? Description { get; set; }

    public int Price { get; set; }

    [MaxLength(300)]
    public string? IconPath { get; set; }

    public int ItemRarityId { get; set; }

    [ForeignKey("ItemRarityId")]
    public virtual ItemRarity ItemRarity { get; set; }
}
