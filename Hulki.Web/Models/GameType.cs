using System.ComponentModel.DataAnnotations;

namespace Hulki.Web.Models;

public class GameType
{
    [Key]
    public int Id { get; set; }
    
    [Required]
    [MaxLength(50)]
    public string Name { get; set; }
    
    [MaxLength(250)]
    public string? Description { get; set; }

    public virtual ICollection<Game>? Games { get; set; }
}