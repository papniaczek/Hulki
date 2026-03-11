using System.ComponentModel.DataAnnotations;

namespace Hulki.Web.Models;

public class ItemRarity
{
    [Key]
    public int Id { get; set; }
    
    [Required]
    [MaxLength(50)]
    public string Name { get; set; } 
    
    [MaxLength(7)]
    public string? HexColor { get; set; } 
    
    public virtual ICollection<RewardItem>? RewardItems { get; set; }
}