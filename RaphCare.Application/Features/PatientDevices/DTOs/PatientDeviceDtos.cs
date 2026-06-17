namespace RaphCare.Application.Features.PatientDevices.DTOs;

public sealed class PatientDeviceListItemDto
{
    public Guid DeviceId { get; set; }
    public string SerialNumber { get; set; } = string.Empty;
    public string Model { get; set; } = string.Empty;
    public DateTime AssignedAt { get; set; }
}

public sealed class RegisterMyDeviceResponseDto
{
    public Guid DeviceId { get; set; }
}

public sealed class SyncMyDeviceReadingsResponseDto
{
    public int HeartRateCount { get; set; }
    public int SpO2Count { get; set; }
}

public sealed class HeartRatePointDto
{
    public DateTime RecordedAt { get; set; }
    public int BeatsPerMinute { get; set; }
}

public sealed class Spo2PointDto
{
    public DateTime RecordedAt { get; set; }
    public decimal SpO2 { get; set; }
    public decimal? PulseRate { get; set; }
}
