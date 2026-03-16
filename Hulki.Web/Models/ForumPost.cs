using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Hulki.Web.Models;

public class ForumPost
{
    [Key]
    public Guid Id { get; set; }

    [Required]
    public string Content { get; set; }

    public DateTime CreatedAt { get; set; } = DateTime.Now;

    // Kto napisał odpowiedź
    public string AppUserId { get; set; }
    [ForeignKey("AppUserId")]
    public virtual AppUser AppUser { get; set; }

    // W jakim temacie
    public Guid ForumTopicId { get; set; }
    [ForeignKey("ForumTopicId")]
    public virtual ForumTopic ForumTopic { get; set; }
}