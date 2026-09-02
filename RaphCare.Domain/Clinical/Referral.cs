using RaphCare.Domain.Common;

namespace RaphCare.Domain.Clinical;

/// <summary>Outbound referral from this hospital. Staff track it until completed or cancelled.</summary>
public class Referral : BaseEntity
{
    public Guid ClinicId { get; set; }
    public Guid PatientId { get; set; }
    public Guid? VisitId { get; set; }

    /// <summary>Destination facility or clinician (free text).</summary>
    public string ReferredTo { get; set; } = string.Empty;

    public string? Reason { get; set; }
    public string? Specialty { get; set; }
    public string? Notes { get; set; }

    /// <summary>Sent, Accepted, Completed, or Cancelled.</summary>
    public string Status { get; set; } = "Sent";

    public DateTime ReferredAt { get; set; }
    public DateTime? AcceptedAt { get; set; }
    public DateTime? CompletedAt { get; set; }
    public Guid? CreatedByApplicationUserId { get; set; }

    public Visit? Visit { get; set; }
}
