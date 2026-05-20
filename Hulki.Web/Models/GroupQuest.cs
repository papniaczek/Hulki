using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Hulki.Web.Models;

public class GroupQuest
{
    [Key]
    public int Id { get; set; }

    public int TherapyGroupId { get; set; }
    [ForeignKey("TherapyGroupId")]
    public virtual TherapyGroup TherapyGroup { get; set; }

    [Required]
    [MaxLength(100)]
    public string Title { get; set; }

    [Required]
    public string Description { get; set; }

    [Required]
    public string QuestType { get; set; } // "Otwarte" lub "QuizABCD"

    public int RewardPoints { get; set; }

    public DateTime CreatedAt { get; set; } = DateTime.Now;

    // --- NOWE POLA DLA QUIZU ABCD ---
    public string? OptionA { get; set; }
    public string? OptionB { get; set; }
    public string? OptionC { get; set; }
    public string? OptionD { get; set; }
    public string? CorrectOption { get; set; } // np. "A", "B", "C" lub "D"

    public virtual ICollection<QuestSubmission> Submissions { get; set; }
}