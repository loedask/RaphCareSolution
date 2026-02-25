using RaphCare.Domain.Common;

namespace RaphCare.Domain.InfrastructureEntities;

/// <summary>
/// Backup operation record.
/// </summary>
public class BackupRecord : BaseEntity
{
    public string BackupLocation { get; set; } = string.Empty;
    public bool Successful { get; set; }
}

