using System.ComponentModel.DataAnnotations;

namespace Hulki.Web.Models.Dto;

public class CreateConsultationDto
{
    [Required(ErrorMessage = "Musisz wybrać osobę.")]
    public string TargetUserId { get; set; }

    [Required]
    public DateTime StartTime { get; set; } = DateTime.Now.AddDays(1);

    [Required]
    public DateTime EndTime { get; set; } = DateTime.Now.AddDays(1).AddHours(1);

    [MaxLength(500)]
    public string? Notes { get; set; }
}