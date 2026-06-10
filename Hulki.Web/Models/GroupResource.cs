using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Hulki.Web.Models;

public class GroupResource
{
    [Key]
    public int Id { get; set; }

    [Required]
    [MaxLength(100)]
    public string Title { get; set; }

    [Required]
    public string FilePath { get; set; }

    public DateTime CreatedAt { get; set; } = DateTime.Now;

    public int TherapyGroupId { get; set; }

    [ForeignKey("TherapyGroupId")]
    public virtual TherapyGroup TherapyGroup { get; set; }
}
