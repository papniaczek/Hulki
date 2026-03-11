using System.ComponentModel.DataAnnotations;

namespace Hulki.Web.Models;

public class FileType
{
    [Key]
    public int Id { get; set; }
    
    [Required]
    [MaxLength(50)]
    public string Name { get; set; }
    
    [Required]
    [MaxLength(10)]
    public string Extension { get; set; }
    public virtual ICollection<ReportAttachment>? ReportAttachments { get; set; }
}