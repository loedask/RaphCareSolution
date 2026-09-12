using RaphCare.Domain.Common;

namespace RaphCare.Domain.Clinical;

/// <summary>Outpatient consult queue ticket. Public displays show the queue code only.</summary>
public class ConsultTicket : BaseEntity
{
    public Guid ClinicId { get; set; }
    public Guid? AppointmentId { get; set; }
    public Guid? PatientId { get; set; }
    public Guid? ProviderId { get; set; }

    /// <summary>Six-character code shown on the waiting screen.</summary>
    public string QueueCode { get; set; } = string.Empty;

    /// <summary>Waiting, Called, Completed, or Cancelled.</summary>
    public string Status { get; set; } = "Waiting";

    public DateTime ArrivedAt { get; set; }
    public DateTime? CalledAt { get; set; }
    public DateTime? CompletedAt { get; set; }
    public Guid? CreatedByApplicationUserId { get; set; }
}
