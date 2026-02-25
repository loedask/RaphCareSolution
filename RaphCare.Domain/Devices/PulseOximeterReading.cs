using RaphCare.Domain.Common;

namespace RaphCare.Domain.Devices;

/// <summary>
/// Pulse oximeter (SpO2) reading from a device.
/// </summary>
public class PulseOximeterReading : DeviceReading
{
    public decimal SpO2 { get; set; }
    public decimal PulseRate { get; set; }
}
