using RaphCare.Domain.Common;

namespace RaphCare.Domain.Clinical;

/// <summary>Scheduled operating-theatre case for a hospital staff board.</summary>
public class TheatreCase : BaseEntity
{
    public Guid ClinicId { get; set; }
    public Guid PatientId { get; set; }
    public DateTime ScheduledStart { get; set; }
    public DateTime? ScheduledEnd { get; set; }
    public string ProcedureName { get; set; } = string.Empty;
    public string? TheatreName { get; set; }
    public string? SurgeonName { get; set; }

    /// <summary>Scheduled, InProgress, Completed, or Cancelled.</summary>
    public string Status { get; set; } = "Scheduled";

    public string? Notes { get; set; }
    public Guid? CreatedByApplicationUserId { get; set; }
}
