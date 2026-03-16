using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Hulki.Web.Models;

public class ReportAttachment
{
    [Key]
    public Guid Id { get; set; }

    public string FileName { get; set; }
    public string FilePath { get; set; }

    public int FileTypeId { get; set; }
    [ForeignKey("FileTypeId")]
    public virtual FileType FileType { get; set; }

    public Guid DailyReportId { get; set; }
    [ForeignKey("DailyReportId")]
    public virtual DailyReport DailyReport { get; set; }
}