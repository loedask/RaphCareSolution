using RaphCare.Domain.Common;

namespace RaphCare.Domain.Organization;

/// <summary>One staff member assigned to a shift on a hospital calendar day.</summary>
public class ClinicRosterEntry : BaseEntity
{
    public Guid ClinicId { get; set; }
    public Guid ApplicationUserId { get; set; }

    /// <summary>Duty date as UTC midnight (date only).</summary>
    public DateTime DutyDate { get; set; }

    /// <summary>Morning, Afternoon, or Night.</summary>
    public string ShiftLabel { get; set; } = "Morning";

    public string? Note { get; set; }
    public Guid? CreatedByApplicationUserId { get; set; }
}
