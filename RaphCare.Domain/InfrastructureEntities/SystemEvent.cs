using RaphCare.Domain.Common;

namespace RaphCare.Domain.InfrastructureEntities;

/// <summary>
/// System-level event for observability and diagnostics.
/// </summary>
public class SystemEvent : BaseEntity
{
    public string EventType { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public DateTime OccurredAt { get; set; }
}

