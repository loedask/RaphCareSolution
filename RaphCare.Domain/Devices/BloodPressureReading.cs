using RaphCare.Domain.Common;

namespace RaphCare.Domain.Devices;

/// <summary>
/// Blood pressure reading from a device.
/// </summary>
public class BloodPressureReading : DeviceReading
{
    public decimal Systolic { get; set; }
    public decimal Diastolic { get; set; }
}
