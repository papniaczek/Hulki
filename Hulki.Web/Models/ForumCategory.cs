using System.ComponentModel.DataAnnotations;

namespace Hulki.Web.Models;

public class ForumCategory
{
    [Key]
    public int Id { get; set; }
    
    [Required]
    [MaxLength(100)]
    public string Name { get; set; }
    
    [MaxLength(500)]
    public string? Description { get; set; }
    
    public virtual ICollection<ForumTopic>? ForumTopics { get; set; }
}