using RaphCare.Domain.Common;

namespace RaphCare.Domain.Devices;

/// <summary>
/// Alert generated when a reading threshold is breached (supports AI-triggered alerts).
/// </summary>
public class DeviceAlert : BaseEntity
{
    public Guid DeviceId { get; set; }
    public Guid PatientId { get; set; }
    public string AlertType { get; set; } = string.Empty;
    public string Message { get; set; } = string.Empty;
    public bool IsResolved { get; set; }
    public DateTime TriggeredAt { get; set; }
    public DateTime? ResolvedAt { get; set; }

    public Device Device { get; set; } = null!;
}
