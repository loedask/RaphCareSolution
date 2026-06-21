using Microsoft.EntityFrameworkCore;
using RaphCare.Application.Common.Interfaces;
using RaphCare.Domain.Patients;

namespace RaphCare.Persistence;

public sealed class PatientLookupService(ClinicalDbContext clinicalDbContext) : IPatientLookupService
{
    public async Task<Patient?> FindByEmailAsync(string email, CancellationToken cancellationToken = default)
    {
        var normalized = email.Trim().ToLowerInvariant();
        return await clinicalDbContext.Patients
            .AsNoTracking()
            .Where(p => !p.IsDeleted && p.Email != null && p.Email.ToLower() == normalized)
            .FirstOrDefaultAsync(cancellationToken)
            .ConfigureAwait(false);
    }
}
