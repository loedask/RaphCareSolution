using Microsoft.Extensions.Logging;
using RaphCare.Application.Common.Interfaces;

namespace RaphCare.Infrastructure.Notifications;

/// <summary>Default push sender: logs intent until FCM/APNs credentials and delivery code are added.</summary>
public sealed class NoOpPatientPushNotificationSender(
    ILogger<NoOpPatientPushNotificationSender> logger) : IPatientPushNotificationSender
{
    private readonly ILogger<NoOpPatientPushNotificationSender> _logger = logger;

    public Task SendToPatientAsync(Guid patientId, string title, string body, string notificationType, CancellationToken cancellationToken = default)
    {
        _logger.LogInformation(
            "Push notification (no-op): patient {PatientId} type {Type} title {Title}",
            patientId,
            notificationType,
            title);
        return Task.CompletedTask;
    }
}
