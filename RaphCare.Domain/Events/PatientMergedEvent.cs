using MediatR;

namespace RaphCare.Domain.Events;

/// <summary>
/// Domain event raised when two patient records are successfully merged (duplicate into primary).
/// Handlers may log to Application Insights, invalidate caches, trigger MPI reconciliation, or notify external integrations.
/// </summary>
public record PatientMergedEvent(
    Guid PrimaryPatientId,
    Guid MergedPatientId,
    DateTime MergedAt,
    Guid? MergedByUserId) : INotification;
