using RaphCare.Domain.Common;

namespace RaphCare.Domain.Patients;

/// <summary>Registered device token for mobile push (FCM/APNs); upserted when the app calls the patient push-registration API.</summary>
public sealed class PatientPushDevice : BaseEntity
{
    public Guid PatientId { get; set; }

    public string DeviceToken { get; set; } = string.Empty;

    /// <summary>Normalized platform: <c>android</c>, <c>ios</c>, <c>web</c>, <c>unknown</c>.</summary>
    public string Platform { get; set; } = string.Empty;

    /// <summary>Refreshes audit metadata when the same token is re-registered.</summary>
    public void TouchRegistration() => SetUpdated();
}
