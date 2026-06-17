using System;
using System.ComponentModel.DataAnnotations;

namespace Hulki.Web.Models;

/// <summary>
/// Log audytowy usuniętych grup terapeutycznych.
/// Wypełniany automatycznie przez trigger trg_AuditTherapyGroupDelete
/// (AFTER DELETE na TherapyGroups) — nie wstawiany ręcznie z C#.
/// </summary>
public class TherapyGroupDeletionLog
{
    [Key]
    public int Id { get; set; }

    public int DeletedGroupId { get; set; }

    [MaxLength(100)]
    public string DeletedGroupName { get; set; } = string.Empty;

    public DateTime DeletedAt { get; set; } = DateTime.UtcNow;
}