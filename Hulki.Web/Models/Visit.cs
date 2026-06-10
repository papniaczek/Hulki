using System;

namespace Hulki.Web.Models  // <-- Zmień to tutaj!
{
    public class Visit
    {
        public Guid Id { get; set; }
        public DateTime ScheduledDate { get; set; }
        public int PatientId { get; set; }
        public int DoctorId { get; set; }
        public VisitStatus Status { get; set; }
        public VisitDetails Details { get; set; } // Teraz zobaczy tę klasę
    }

    public enum VisitStatus
    {
        Scheduled,
        Cancelled,
        Completed
    }
}