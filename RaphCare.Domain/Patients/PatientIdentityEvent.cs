using RaphCare.Domain.Common;
using RaphCare.Domain.Patients.Enums;

namespace RaphCare.Domain.Patients;

/// <summary>
/// Immutable audit record for patient identity lifecycle changes (create, link, unlink, update, merge).
/// Used for forensic auditability and operational diagnostics.
/// </summary>
public class PatientIdentityEvent : BaseEntity
{
    public Guid PatientId { get; set; }
    public PatientIdentityEventType EventType { get; set; }
    public string EventDataJson { get; set; } = string.Empty;
    public DateTime OccurredAt { get; set; }
    public Guid? PerformedByUserId { get; set; }

    /// <summary>
    /// Optional correlation id (request trace id, job id, etc).
    /// </summary>
    public string? CorrelationId { get; set; }
}

