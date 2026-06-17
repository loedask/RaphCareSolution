using RaphCare.Domain.Common;

namespace RaphCare.Domain.Organization;

/// <summary>
/// Blocked or special availability slot within a provider schedule.
/// </summary>
///
/// <remarks>
/// Relationship: child of <see cref="ProviderSchedule"/> and represents a blocked time range (via <c>StartDateTime</c> and <c>EndDateTime</c>), optionally explained by <c>Reason</c>.
/// </remarks>
public class AvailabilityBlock : BaseEntity
{
    public Guid ProviderScheduleId { get; set; }
    public DateTime StartDateTime { get; set; }
    public DateTime EndDateTime { get; set; }
    public string? Reason { get; set; }
    public bool IsBlocked { get; set; }

    public ProviderSchedule ProviderSchedule { get; set; } = null!;
}
