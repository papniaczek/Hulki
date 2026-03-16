using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Hulki.Web.Models;

public class ForumTopic
{
    [Key]
    public Guid Id { get; set; }

    [Required(ErrorMessage = "Tytuł wątku jest wymagany")]
    [MaxLength(200)]
    public string Title { get; set; }

    [Required(ErrorMessage = "Treść jest wymagana")]
    public string Content { get; set; }

    public DateTime CreatedAt { get; set; } = DateTime.Now;

    // Kto założył temat
    public string AppUserId { get; set; }
    [ForeignKey("AppUserId")]
    public virtual AppUser AppUser { get; set; }

    // W jakiej kategorii
    public int ForumCategoryId { get; set; }
    [ForeignKey("ForumCategoryId")]
    public virtual ForumCategory ForumCategory { get; set; }
        
    public virtual ICollection<ForumPost> Posts { get; set; }
}