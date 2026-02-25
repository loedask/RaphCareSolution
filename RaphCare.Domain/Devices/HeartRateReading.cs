using RaphCare.Domain.Common;

namespace RaphCare.Domain.Devices;

/// <summary>
/// Heart rate reading from a device.
/// </summary>
public class HeartRateReading : DeviceReading
{
    public decimal HeartRate { get; set; }
}
