using RaphCare.Domain.Patients;

namespace RaphCare.Application.Common.Interfaces;

public interface IPatientLookupService
{
    Task<Patient?> FindByEmailAsync(string email, CancellationToken cancellationToken = default);
}
