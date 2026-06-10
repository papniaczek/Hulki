public class UpdateVisitDetailsDto
{
    public string MedicalHistory { get; set; }
    public string Diagnosis { get; set; }
    public string Recommendations { get; set; }
    public string InternalNotes { get; set; }
    public List<MedicationDto> Medications { get; set; }
}

public class MedicationDto
{
    public string Name { get; set; }
    public string Dosage { get; set; }
    public string Duration { get; set; }
}
