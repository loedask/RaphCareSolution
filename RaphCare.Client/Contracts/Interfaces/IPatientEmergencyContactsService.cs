using RaphCare.Client.Contracts;
using RaphCare.Client.Models.EmergencyContacts;

namespace RaphCare.Client.Contracts.Interfaces;

/// <summary>Patient ICE contacts (<c>api/patient/emergency-contacts</c>).</summary>
public interface IPatientEmergencyContactsService
{
    Task<Response<IReadOnlyList<PatientEmergencyContactViewModel>>> GetMyEmergencyContactsAsync(CancellationToken cancellationToken = default);

    Task<Response<PatientEmergencyContactViewModel>> GetMyEmergencyContactAsync(Guid id, CancellationToken cancellationToken = default);

    Task<Response<Guid>> AddEmergencyContactAsync(
        string name,
        string? relationship,
        string? phoneNumber,
        string? email,
        CancellationToken cancellationToken = default);

    Task<Response<bool>> UpdateEmergencyContactAsync(
        Guid id,
        string name,
        string? relationship,
        string? phoneNumber,
        string? email,
        CancellationToken cancellationToken = default);

    Task<Response<bool>> RemoveEmergencyContactAsync(Guid id, CancellationToken cancellationToken = default);
}
