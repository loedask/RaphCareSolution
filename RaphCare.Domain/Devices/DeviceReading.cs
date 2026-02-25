using RaphCare.Domain.Common;

namespace RaphCare.Domain.Devices;

/// <summary>
/// Base entity for device readings. Supports inheritance for high-volume ingestion.
/// </summary>
public abstract class DeviceReading : BaseEntity
{
    public Guid DeviceId { get; set; }
    public Guid PatientId { get; set; }
    public DateTime RecordedAt { get; set; }
    public DateTime ReceivedAt { get; set; }
    public string ReadingType { get; set; } = string.Empty;
    public decimal PrimaryValue { get; set; }
    public string Unit { get; set; } = string.Empty;
    public bool IsFlagged { get; set; }

    public Device Device { get; set; } = null!;
}
