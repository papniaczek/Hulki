using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Hulki.Web.Models;

public class DailyReport
{
    [Key]
    public Guid Id { get; set; }

    [Required(ErrorMessage = "Treść raportu jest wymagana")]
    public string Content { get; set; }

    public DateTime CreatedAt { get; set; } = DateTime.Now;

    public int ReportStatusId { get; set; }
    [ForeignKey("ReportStatusId")]
    public virtual ReportStatus ReportStatus { get; set; }

    public string AppUserId { get; set; }
    [ForeignKey("AppUserId")]
    public virtual AppUser AppUser { get; set; }
}