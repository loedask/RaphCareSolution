using RaphCare.Domain.Common;

namespace RaphCare.Domain.Devices;

/// <summary>
/// Standalone cellular device event (e.g. Y6-class 4G): SOS, fall, or location ping. Ingested server-side; not dependent on the patient smartphone.
/// </summary>
public class DeviceEmergencyEvent : BaseEntity
{
    public Guid DeviceId { get; set; }
    public Guid PatientId { get; set; }
    public Guid ClinicId { get; set; }
    public string EventType { get; set; } = string.Empty;
    public DateTime OccurredAtUtc { get; set; }
    public DateTime ReceivedAtUtc { get; set; }
    public double? Latitude { get; set; }
    public double? Longitude { get; set; }
    public double? HorizontalAccuracyMeters { get; set; }
    /// <summary>Vendor-supplied id for idempotency (optional).</summary>
    public string? ExternalCorrelationId { get; set; }
    public bool CaregiversNotified { get; set; }
    public DateTime? CaregiversNotifiedAtUtc { get; set; }
    /// <summary>Short outcome, e.g. SMS count or error hint.</summary>
    public string? CaregiverNotificationSummary { get; set; }

    public Device Device { get; set; } = null!;
}
