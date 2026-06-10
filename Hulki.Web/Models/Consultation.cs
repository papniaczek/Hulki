namespace Hulki.Web.Models;

public class Consultation
{
    public Guid Id { get; set; }
    public string PatientId { get; set; }
    public virtual AppUser Patient { get; set; }
    public string TherapistId { get; set; }
    public virtual AppUser Therapist { get; set; }
    public DateTime StartTime { get; set; }
    public DateTime EndTime { get; set; }
    public string? Notes { get; set; }
    public int StatusId { get; set; }
    public virtual ConsultationStatus Status { get; set; }
    public VisitDetails Details { get; set; }
}
