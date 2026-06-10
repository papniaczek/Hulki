using System.Collections.Generic;

namespace Hulki.Web.Models  // <-- Zmień to tutaj!
{
    public class VisitDetails
    {
        public int Id { get; set; } 
        public Guid VisitId { get; set; }
        public Guid ConsultationId { get; set; }
        public string MedicalHistory { get; set; }
        public string Diagnosis { get; set; }
        public string Recommendations { get; set; }
        public List<PrescribedMedication> Medications { get; set; } = new();
        public string InternalNotes { get; set; }
    }

    public class PrescribedMedication
    {
        public int Id { get; set; }
        public string Name { get; set; }
        public string Dosage { get; set; }
        public string Duration { get; set; }
    }
}