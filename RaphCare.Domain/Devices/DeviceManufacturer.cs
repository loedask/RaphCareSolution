using RaphCare.Domain.Common;

namespace RaphCare.Domain.Devices;

/// <summary>
/// Manufacturer of medical devices.
/// </summary>
public class DeviceManufacturer : BaseEntity
{
    public string Name { get; set; } = string.Empty;
    public string? Country { get; set; }
    public string? SupportContact { get; set; }
}
