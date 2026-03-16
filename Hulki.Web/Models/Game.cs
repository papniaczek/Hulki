using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Hulki.Web.Models;

public class Game
{
    [Key]
    public Guid Id { get; set; }

    [Required(ErrorMessage = "Nazwa gry/skrzynki jest wymagana")]
    public string Name { get; set; } // np. "Skrzynia Początkującego"

    public string Description { get; set; }
        
    public int Cost { get; set; } // Ile punktów terapii kosztuje zagranie/otwarcie

    // Typ gry (np. Ruletka, Lootbox)
    public int GameTypeId { get; set; }
    [ForeignKey("GameTypeId")]
    public virtual GameType GameType { get; set; }
}