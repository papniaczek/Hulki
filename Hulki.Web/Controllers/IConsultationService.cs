using Hulki.Web.Models;

namespace Hulki.Web.Services;

public interface IConsultationService
{
    Task<IEnumerable<Consultation>> GetPatientConsultationsAsync(string patientId);
    Task<IEnumerable<Consultation>> GetTherapistConsultationsAsync(string therapistId);
    Task CreateConsultationAsync(Consultation consultation);
    Task UpdateStatusAsync(Guid consultationId, int newStatusId);

    // --- DOPISZ TE DWIE METODY ---
    Task<Consultation> GetConsultationByIdAsync(Guid id);
    Task UpdateConsultationAsync(Consultation consultation);
}