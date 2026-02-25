using RaphCare.Domain.Common;

namespace RaphCare.Domain.Devices;

/// <summary>
/// Assignment of a device to a patient.
/// </summary>
public class DeviceAssignment : BaseEntity
{
    public Guid DeviceId { get; set; }
    public Guid PatientId { get; set; }
    public DateTime AssignedAt { get; set; }
    public DateTime? ReturnedAt { get; set; }
    public bool IsActive { get; set; }

    public Device Device { get; set; } = null!;
}
