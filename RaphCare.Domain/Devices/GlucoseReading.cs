using RaphCare.Domain.Common;

namespace RaphCare.Domain.Devices;

/// <summary>
/// Blood glucose reading from a device.
/// </summary>
public class GlucoseReading : DeviceReading
{
    public decimal GlucoseLevel { get; set; }
    public bool IsFasting { get; set; }
}
