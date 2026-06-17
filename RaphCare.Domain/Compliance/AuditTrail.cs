using RaphCare.Domain.Common;

namespace RaphCare.Domain.Compliance;

/// <summary>
/// Immutable audit trail entry for compliance and traceability.
/// </summary>
public class AuditTrail : BaseEntity
{
    public Guid UserId { get; set; }
    public string Action { get; set; } = string.Empty;
    public string EntityName { get; set; } = string.Empty;
    public string ChangesJson { get; set; } = string.Empty;
    public DateTime PerformedAt { get; set; }
}
