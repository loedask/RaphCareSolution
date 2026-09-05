namespace RaphCare.Client.Models.Devices;

public sealed class PatientDeviceListItemViewModel
{
    public Guid DeviceId { get; set; }
    public string SerialNumber { get; set; } = string.Empty;
    public string Model { get; set; } = string.Empty;
    public string? BluetoothMacAddress { get; set; }
    public DateTime AssignedAt { get; set; }
}

public sealed class RegisterMyDeviceResultViewModel
{
    public Guid DeviceId { get; set; }
    public string? BluetoothMacAddress { get; set; }
}

public sealed class BindMyDeviceBluetoothMacResultViewModel
{
    public Guid DeviceId { get; set; }
    public string BluetoothMacAddress { get; set; } = string.Empty;
}

public sealed class SyncMyDeviceReadingsResultViewModel
{
    public int HeartRateCount { get; set; }
    public int SpO2Count { get; set; }
}

/// <summary>Latest synced wearable vitals for the signed-in patient (Home health summary).</summary>
public sealed class PatientLatestReadingsViewModel
{
    public decimal? HeartRateBpm { get; set; }
    public DateTime? HeartRateRecordedAt { get; set; }
    public decimal? SpO2Percent { get; set; }
    public DateTime? SpO2RecordedAt { get; set; }
}

/// <summary>Input for batch HR sync (maps to generated API body).</summary>
public sealed class HeartRateReadingInput
{
    public DateTime RecordedAt { get; set; }
    public int BeatsPerMinute { get; set; }
}

/// <summary>Input for batch SpO₂ sync (maps to generated API body).</summary>
public sealed class Spo2ReadingInput
{
    public DateTime RecordedAt { get; set; }
    public decimal SpO2 { get; set; }
    public decimal? PulseRate { get; set; }
}
