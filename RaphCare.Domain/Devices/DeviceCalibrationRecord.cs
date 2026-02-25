using RaphCare.Domain.Common;

namespace RaphCare.Domain.Devices;

/// <summary>
/// Calibration record for a device.
/// </summary>
public class DeviceCalibrationRecord : BaseEntity
{
    public Guid DeviceId { get; set; }
    public DateTime CalibratedAt { get; set; }
    public string? CalibrationNotes { get; set; }
    public string? CalibratedBy { get; set; }

    public Device Device { get; set; } = null!;
}
