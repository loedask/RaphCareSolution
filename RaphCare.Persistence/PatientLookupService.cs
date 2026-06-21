using Microsoft.EntityFrameworkCore;
using RaphCare.Application.Common.Interfaces;
using RaphCare.Domain.Patients;

namespace RaphCare.Persistence;

public sealed class PatientLookupService(ClinicalDbContext clinicalDbContext) : IPatientLookupService
{
    public async Task<Patient?> FindByEmailAsync(string email, CancellationToken cancellationToken = default)
    {
        var trimmedEmail = email.Trim();
        return await clinicalDbContext.Patients
            .AsNoTracking()
            .Where(p => !p.IsDeleted && p.Email != null && string.Equals(p.Email, trimmedEmail, StringComparison.OrdinalIgnoreCase))
            .FirstOrDefaultAsync(cancellationToken)
            .ConfigureAwait(false);
    }
}
