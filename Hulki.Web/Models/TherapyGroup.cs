using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Hulki.Web.Models;

public class TherapyGroup
{
    [Key]
    public int Id { get; set; }

    [Required]
    [MaxLength(100)]
    public string Name { get; set; }

    [MaxLength(500)]
    public string Description { get; set; }

    [Required]
    [Range(1, 100, ErrorMessage = "Grupa musi mieć od 1 do 100 miejsc.")]
    public int MaxParticipants { get; set; } = 10;

    public int TherapyTypeId { get; set; }

    [ForeignKey("TherapyTypeId")]
    public virtual TherapyType TherapyType { get; set; }

    public virtual ICollection<PatientGroup> PatientGroups { get; set; }
}
