using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Hulki.Web.Models;

public class QuestSubmission
{
    [Key]
    public int Id { get; set; }

    public int GroupQuestId { get; set; }
    [ForeignKey("GroupQuestId")]
    public virtual GroupQuest GroupQuest { get; set; }

    public string AppUserId { get; set; }
    [ForeignKey("AppUserId")]
    public virtual AppUser AppUser { get; set; }

    [Required]
    public string AnswerText { get; set; }

    public DateTime SubmittedAt { get; set; } = DateTime.Now;

    // Status sprawdzania przez terapeutę
    public bool IsEvaluated { get; set; } = false;
    public bool IsAccepted { get; set; } = false;
}