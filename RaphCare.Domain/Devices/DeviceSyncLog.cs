using RaphCare.Domain.Common;

namespace RaphCare.Domain.Devices;

/// <summary>
/// Tracks ingestion/sync events for a device.
/// </summary>
public class DeviceSyncLog : BaseEntity
{
    public Guid DeviceId { get; set; }
    public DateTime SyncStartedAt { get; set; }
    public DateTime? SyncCompletedAt { get; set; }
    public int RecordsSynced { get; set; }
    public bool Successful { get; set; }
    public string? ErrorMessage { get; set; }

    public Device Device { get; set; } = null!;
}
