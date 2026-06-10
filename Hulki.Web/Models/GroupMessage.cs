using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Hulki.Web.Models;

public class GroupMessage
{
    [Key]
    public int Id { get; set; }

    [Required]
    public string Content { get; set; }

    public DateTime CreatedAt { get; set; } = DateTime.Now;

    public string AppUserId { get; set; }

    [ForeignKey("AppUserId")]
    public virtual AppUser AppUser { get; set; }

    public int TherapyGroupId { get; set; }

    [ForeignKey("TherapyGroupId")]
    public virtual TherapyGroup TherapyGroup { get; set; }
}
