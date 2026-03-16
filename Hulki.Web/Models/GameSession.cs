using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Hulki.Web.Models;

public class GameSession
{
    [Key]
    public Guid Id { get; set; }

    public DateTime PlayedAt { get; set; } = DateTime.Now;

    // Kto zagrał
    public string AppUserId { get; set; }
    [ForeignKey("AppUserId")]
    public virtual AppUser AppUser { get; set; }

    // W co zagrał
    public Guid GameId { get; set; }
    [ForeignKey("GameId")]
    public virtual Game Game { get; set; }
}