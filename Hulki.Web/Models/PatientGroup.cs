using System.ComponentModel.DataAnnotations.Schema;

namespace Hulki.Web.Models;

public class PatientGroup
{
    public string AppUserId { get; set; }
    
    [ForeignKey("AppUserId")]
    public virtual AppUser AppUser { get; set; }
    
    public int TherapyGroupId { get; set; }
    
    [ForeignKey("TherapyGroupId")]
    public virtual TherapyGroup TherapyGroup { get; set; }
    
    public DateTime JoinedDate { get; set; } = DateTime.Now;
}