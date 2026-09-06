using RaphCare.Client.Contracts;
using RaphCare.Client.Models.Clinics;

namespace RaphCare.Client.Contracts.Interfaces;

/// <summary>Patient linked hospitals (<c>api/patient/clinics</c>).</summary>
public interface IPatientClinicsService
{
    Task<Response<IReadOnlyList<PatientLinkedClinicViewModel>>> GetMyLinkedClinicsAsync(
        CancellationToken cancellationToken = default);
}
