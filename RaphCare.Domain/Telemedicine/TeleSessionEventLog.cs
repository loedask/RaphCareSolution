using RaphCare.Domain.Common;

namespace RaphCare.Domain.Telemedicine;

/// <summary>
/// System-level event for a telemedicine session (debugging and compliance).
/// </summary>
public class TeleSessionEventLog : BaseEntity
{
    public Guid TeleSessionId { get; set; }
    public string EventType { get; set; } = string.Empty; // Joined / Left / Error / Reconnected
    public string? Description { get; set; }
    public DateTime OccurredAt { get; set; }

    public TeleSession TeleSession { get; set; } = null!;
}
