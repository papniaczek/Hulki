using System.ComponentModel.DataAnnotations;

namespace Hulki.Web.Models;

public class ReportStatus
{
    [Key]
    public int Id { get; set; }
    
    [Required]
    [MaxLength(50)]
    public string Name { get; set; }
    public virtual ICollection<DailyReport>? DailyReports { get; set; }
}