namespace RaphCare.Application.Features.StandaloneEmergency.DTOs;

public sealed class PatientDeviceEmergencyEventListItemDto
{
    public Guid Id { get; set; }
    public Guid DeviceId { get; set; }
    public string SerialNumber { get; set; } = string.Empty;
    public string Model { get; set; } = string.Empty;
    public string EventType { get; set; } = string.Empty;
    public DateTime OccurredAtUtc { get; set; }
    public DateTime ReceivedAtUtc { get; set; }
    public double? Latitude { get; set; }
    public double? Longitude { get; set; }
    public double? HorizontalAccuracyMeters { get; set; }
    public string? ExternalCorrelationId { get; set; }
    public bool CaregiversNotified { get; set; }
    public DateTime? CaregiversNotifiedAtUtc { get; set; }
    public string? CaregiverNotificationSummary { get; set; }
}
