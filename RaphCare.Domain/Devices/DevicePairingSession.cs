using RaphCare.Domain.Common;

namespace RaphCare.Domain.Devices;

/// <summary>
/// Mobile pairing attempt for a device.
/// </summary>
public class DevicePairingSession : BaseEntity
{
    public Guid DeviceId { get; set; }
    public Guid PatientId { get; set; }
    public DateTime StartedAt { get; set; }
    public DateTime? CompletedAt { get; set; }
    public bool Successful { get; set; }
    public string PairingMethod { get; set; } = string.Empty; // Bluetooth / WiFi

    public Device Device { get; set; } = null!;
}
