using RaphCare.Application.Common.DTOs;

namespace RaphCare.Application.Common.Interfaces;

/// <summary>Reads registered device tokens for FCM/APNs delivery.</summary>
public interface IPatientPushDeviceTokenReader
{
    /// <summary>Returns tokens registered for the patient (may be empty).</summary>
    Task<IReadOnlyList<PatientPushDeviceToken>> GetTokensForPatientAsync(
        Guid patientId,
        CancellationToken cancellationToken = default);
}
