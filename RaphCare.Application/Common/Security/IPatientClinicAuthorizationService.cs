using RaphCare.Domain.Patients;

namespace RaphCare.Application.Common.Security;

public interface IPatientClinicAuthorizationService
{
    Task<Patient> GetCurrentPatientAsync(CancellationToken cancellationToken);
    Task EnsurePatientClinicAccessAsync(Guid clinicId, CancellationToken cancellationToken);
}

