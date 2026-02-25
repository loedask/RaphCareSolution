using RaphCare.Domain.Common;

namespace RaphCare.Domain.Organization;

/// <summary>
/// Weekly availability template for a provider.
/// </summary>
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
