using System.ComponentModel.DataAnnotations.Schema;

namespace Hulki.Web.Models;

public class PatientInventory
{
    public string AppUserId { get; set; }
    [ForeignKey("AppUserId")]
    public virtual AppUser AppUser { get; set; }

    public Guid RewardItemId { get; set; }
    [ForeignKey("RewardItemId")]
    public virtual RewardItem RewardItem { get; set; }

    public DateTime AcquiredDate { get; set; } = DateTime.Now;
}