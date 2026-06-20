using MediatR;
using Microsoft.Extensions.Logging;
using RaphCare.Domain.Events;

namespace RaphCare.Application.Common.EventHandlers;

/// <summary>
/// Logs patient merge events for Application Insights (and any logger provider).
/// When Application Insights is configured, these logs appear in traces with structured properties for querying.
/// Future handlers may invalidate caches, trigger MPI reconciliation, or notify external integrations.
/// </summary>
public sealed partial class PatientMergedEventLoggingHandler(ILogger<PatientMergedEventLoggingHandler> logger)
    : INotificationHandler<PatientMergedEvent>
{
    public Task Handle(PatientMergedEvent notification, CancellationToken cancellationToken)
    {
        LogPatientMergeCompleted(
            notification.PrimaryPatientId,
            notification.MergedPatientId,
            notification.MergedAt,
            notification.MergedByUserId);
        return Task.CompletedTask;
    }

    [LoggerMessage(
        Level = LogLevel.Information,
        Message = "Patient merge completed: Primary {PrimaryPatientId}, Merged {MergedPatientId}, MergedAt {MergedAt}, MergedByUserId {MergedByUserId}")]
    private partial void LogPatientMergeCompleted(
        Guid primaryPatientId,
        Guid mergedPatientId,
        DateTime mergedAt,
        Guid? mergedByUserId);
}
