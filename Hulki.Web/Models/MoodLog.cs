using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Hulki.Web.Models;

public class MoodLog
{
    public Guid Id { get; set; }

    [MaxLength(450)]
    public string AppUserId { get; set; }

    [ForeignKey("AppUserId")]
    public virtual AppUser AppUser { get; set; }

    public DateTime Date { get; set; } = DateTime.Now;
    public int MoodTypeId { get; set; }
    public virtual MoodType MoodType { get; set; }
}