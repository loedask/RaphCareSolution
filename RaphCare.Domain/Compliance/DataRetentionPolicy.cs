using RaphCare.Domain.Common;

namespace RaphCare.Domain.Compliance;

/// <summary>
/// Data retention policy per entity type for compliance.
/// </summary>
public class DataRetentionPolicy : BaseEntity
{
    public string EntityName { get; set; } = string.Empty;
    public int RetentionDays { get; set; }
    public bool IsActive { get; set; }
}
