using RaphCare.Domain.Common;

namespace RaphCare.Domain.Telemedicine;

/// <summary>
/// Participant who joined a telemedicine session.
/// </summary>
public class TeleSessionParticipant : BaseEntity
{
    public Guid TeleSessionId { get; set; }
    public Guid UserId { get; set; }
    public string Role { get; set; } = string.Empty; // Patient / Provider / Observer
    public DateTime JoinedAt { get; set; }
    public DateTime? LeftAt { get; set; }
    public string? ConnectionStatus { get; set; }

    public TeleSession TeleSession { get; set; } = null!;
}
