using Microsoft.Extensions.Logging;
using RaphCare.Application.Common.Interfaces;

namespace RaphCare.Infrastructure.Notifications;

/// <summary>Default push sender: logs intent until FCM/APNs credentials and delivery code are added.</summary>
public sealed partial class NoOpPatientPushNotificationSender(
    ILogger<NoOpPatientPushNotificationSender> logger) : IPatientPushNotificationSender
{
    public Task SendToPatientAsync(Guid patientId, string title, string body, string notificationType, CancellationToken cancellationToken = default)
    {
        LogPushNoOp(patientId, notificationType, title);
        return Task.CompletedTask;
    }

    [LoggerMessage(
        Level = LogLevel.Information,
        Message = "Push notification (no-op): patient {PatientId} type {Type} title {Title}")]
    private partial void LogPushNoOp(Guid patientId, string type, string title);
}
