using RaphCare.Domain.Common;

namespace RaphCare.Domain.Devices;

/// <summary>
/// ECG reading from a device.
/// </summary>
public class ECGReading : DeviceReading
{
    public decimal HeartRate { get; set; }
    public string? RhythmClassification { get; set; }
}
