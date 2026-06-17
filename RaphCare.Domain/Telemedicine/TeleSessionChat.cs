using RaphCare.Domain.Common;

namespace RaphCare.Domain.Telemedicine;

/// <summary>
/// Chat message during a telemedicine session (auditable).
/// </summary>
public class TeleSessionChat : BaseEntity
{
    public Guid TeleSessionId { get; set; }
    public Guid SenderUserId { get; set; }
    public string Message { get; set; } = string.Empty;
    public DateTime SentAt { get; set; }
    public bool IsSystemMessage { get; set; }

    public TeleSession TeleSession { get; set; } = null!;
}
