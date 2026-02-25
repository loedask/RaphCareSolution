using RaphCare.Domain.Common;

namespace RaphCare.Domain.Organization;

/// <summary>
/// Blocked or special availability slot within a provider schedule.
/// </summary>
public class AvailabilityBlock : BaseEntity
{
    public Guid ProviderScheduleId { get; set; }
    public DateTime StartDateTime { get; set; }
    public DateTime EndDateTime { get; set; }
    public string? Reason { get; set; }
    public bool IsBlocked { get; set; }

    public ProviderSchedule ProviderSchedule { get; set; } = null!;
}
