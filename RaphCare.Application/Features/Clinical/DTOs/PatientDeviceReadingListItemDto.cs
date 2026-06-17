namespace RaphCare.Application.Features.Clinical.DTOs;

/// <summary>
/// One wearable-derived reading for provider dashboard / charts (device bounded context).
/// </summary>
public sealed class PatientDeviceReadingListItemDto
{
    public Guid Id { get; set; }
    public Guid DeviceId { get; set; }
    public string SerialNumber { get; set; } = string.Empty;
    public string Model { get; set; } = string.Empty;
    /// <summary>Discriminator-style kind, e.g. HeartRate, PulseOximeter.</summary>
    public string Kind { get; set; } = string.Empty;
    public string ReadingType { get; set; } = string.Empty;
    public decimal PrimaryValue { get; set; }
    public string Unit { get; set; } = string.Empty;
    public DateTime RecordedAt { get; set; }
    public DateTime ReceivedAt { get; set; }
    public int? HeartRateBpm { get; set; }
    public decimal? SpO2Percent { get; set; }
    public int? PulseRateBpm { get; set; }
}
