using RaphCare.Domain.Patients.Enums;

namespace RaphCare.Application.Common.Interfaces;

/// <summary>
/// Records patient identity lifecycle events for audit/forensic tracing.
/// </summary>
public interface IPatientIdentityTimelineService
{
    /// <summary>
    /// Records a patient identity timeline event in the clinical persistence model.
    /// </summary>
    Task RecordEventAsync(
        Guid patientId,
        PatientIdentityEventType eventType,
        object? eventData,
        Guid? performedByUserId,
        CancellationToken ct);
}

