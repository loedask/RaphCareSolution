using RaphCare.Domain.Common;

namespace RaphCare.Domain.Organization;

/// <summary>
/// Weekly availability template for a provider.
/// </summary>
/// 
/// <remarks>
/// Relationship: belongs to a specific <see cref="Provider"/> via <c>ProviderId</c> and owns <c>AvailabilityBlocks</c> that define blocked or special slots for schedule exceptions.
/// Aggregate rationale: acts as the boundary for representing a provider's recurring availability plus its exceptions.
/// </remarks>
public class ProviderSchedule : BaseEntity
{
    public Guid ProviderId { get; set; }
    public DayOfWeek Day { get; set; }
    public TimeSpan StartTime { get; set; }
    public TimeSpan EndTime { get; set; }
    public bool IsRecurring { get; set; }

    public Provider Provider { get; set; } = null!;
    public ICollection<AvailabilityBlock> AvailabilityBlocks { get; set; } = new List<AvailabilityBlock>();
}
