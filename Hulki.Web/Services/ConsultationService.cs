using Hulki.Web.Data;
using Hulki.Web.Models;
using Microsoft.EntityFrameworkCore;

namespace Hulki.Web.Services;

public class ConsultationService : IConsultationService
{
    private readonly ApplicationDbContext _context;

    public ConsultationService(ApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<IEnumerable<Consultation>> GetPatientConsultationsAsync(string patientId)
    {
        return await _context.Consultations
                .Include(c => c.Patient)
            .Include(c => c.Therapist)
            .Include(c => c.Status)
            .Where(c => c.PatientId == patientId)
            .OrderBy(c => c.StartTime)
            .ToListAsync();
    }

    public async Task<IEnumerable<Consultation>> GetTherapistConsultationsAsync(string therapistId)
    {
        return await _context.Consultations
            .Include(c => c.Patient)
            .Include(c => c.Therapist)
            .Include(c => c.Status)
            .Where(c => c.TherapistId == therapistId)
            .OrderBy(c => c.StartTime)
            .ToListAsync();
    }

    public async Task CreateConsultationAsync(Consultation consultation)
    {
        _context.Consultations.Add(consultation);
        await _context.SaveChangesAsync();
    }

    public async Task UpdateStatusAsync(Guid consultationId, int newStatusId)
    {
        var consultation = await _context.Consultations.FindAsync(consultationId);
        if (consultation != null)
        {
            consultation.StatusId = newStatusId;
            await _context.SaveChangesAsync();
        }
    }
    public async Task<Consultation> GetConsultationByIdAsync(Guid id)
    {
        return await _context.Consultations
            .Include(c => c.Patient)
            .Include(c => c.Therapist)
            .Include(c => c.Status)
            .Include(c => c.Details)
            .FirstOrDefaultAsync(c => c.Id == id);
    }
    public async Task<IEnumerable<Consultation>> GetUserConsultationsAsync(string userId)
    {
        return await _context.Consultations
            .Include(c => c.Patient)
            .Include(c => c.Therapist)
            .Include(c => c.Status)
            .Where(c => c.PatientId == userId || c.TherapistId == userId)
            .ToListAsync();
    }

    public async Task UpdateConsultationAsync(Consultation consultation)
    {
        _context.Consultations.Update(consultation);
        await _context.SaveChangesAsync();
    }
}