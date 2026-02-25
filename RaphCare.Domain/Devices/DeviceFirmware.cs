using RaphCare.Domain.Common;

namespace RaphCare.Domain.Devices;

/// <summary>
/// Firmware version for a device.
/// </summary>
public class DeviceFirmware : BaseEntity
{
    public string Version { get; set; } = string.Empty;
    public DateTime ReleasedAt { get; set; }
    public string? Notes { get; set; }
}
