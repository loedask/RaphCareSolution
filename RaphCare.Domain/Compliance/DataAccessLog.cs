using RaphCare.Domain.Common;

namespace RaphCare.Domain.Compliance;

/// <summary>
/// Log of data access for compliance and security auditing.
/// </summary>
public class DataAccessLog : BaseEntity
{
    public Guid UserId { get; set; }
    public Guid EntityId { get; set; }
    public string EntityType { get; set; } = string.Empty;
    public DateTime AccessedAt { get; set; }
}
