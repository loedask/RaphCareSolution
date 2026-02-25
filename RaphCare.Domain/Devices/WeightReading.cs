using RaphCare.Domain.Common;

namespace RaphCare.Domain.Devices;

/// <summary>
/// Weight reading from a device.
/// </summary>
public class WeightReading : DeviceReading
{
    public decimal Weight { get; set; }
    public decimal? BMI { get; set; }
}
