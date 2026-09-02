using RaphCare.Domain.Common;

namespace RaphCare.Domain.Clinical;

/// <summary>Walk-in casualty / triage queue ticket. Public displays show the queue code only.</summary>
public class CasualtyTicket : BaseEntity
{
    public Guid ClinicId { get; set; }
    public Guid? PatientId { get; set; }

    /// <summary>Six-character code shown on the waiting screen.</summary>
    public string QueueCode { get; set; } = string.Empty;

    /// <summary>Red, Orange, Yellow, or Green.</summary>
    public string TriageLevel { get; set; } = "Green";

    /// <summary>Staff-only complaint text. Never sent to the public display.</summary>
    public string? ChiefComplaint { get; set; }

    /// <summary>Waiting, Called, Completed, or Cancelled.</summary>
    public string Status { get; set; } = "Waiting";

    public DateTime ArrivedAt { get; set; }
    public DateTime? CalledAt { get; set; }
    public DateTime? CompletedAt { get; set; }
    public Guid? CreatedByApplicationUserId { get; set; }
}
