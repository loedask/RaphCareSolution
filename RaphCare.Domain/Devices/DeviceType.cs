using RaphCare.Domain.Common;

namespace RaphCare.Domain.Devices;

/// <summary>
/// Type/category of device (BP, Glucose, ECG, Weight, etc.).
/// </summary>
public class DeviceType : BaseEntity
{
    public string Name { get; set; } = string.Empty;
    public string Category { get; set; } = string.Empty; // BP / Glucose / ECG / Weight / etc.
    public string? Description { get; set; }
}
