namespace RaphCare.Application.Common.Interfaces;

/// <summary>
/// Sends a push notification to the patient’s registered devices (FCM/APNs). Replace the default no-op when keys and provider wiring exist.
/// </summary>
public interface IPatientPushNotificationSender
{
    /// <summary>Delivers (or logs) a push for the given patient using stored device tokens.</summary>
    Task SendToPatientAsync(Guid patientId, string title, string body, string notificationType, CancellationToken cancellationToken = default);
}
