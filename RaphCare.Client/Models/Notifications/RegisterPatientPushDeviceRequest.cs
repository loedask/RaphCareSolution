namespace RaphCare.Client.Models.Notifications;

/// <summary>Body for <c>PUT api/patient/notifications/push-device</c>.</summary>
public sealed class RegisterPatientPushDeviceRequest
{
    public string DeviceToken { get; set; } = string.Empty;
    public string Platform { get; set; } = string.Empty;
}
