using RaphCare.Domain.Common;

namespace RaphCare.Domain.Communication;

/// <summary>
/// Outbound or logged communication (email, SMS, push).
/// </summary>
public class Message : BaseEntity
{
    public Guid ClinicId { get; set; }
    public string RecipientUserId { get; set; } = string.Empty;
    public string Channel { get; set; } = string.Empty;
    public string? Subject { get; set; }
    public string Body { get; set; } = string.Empty;
    public DateTime? SentAt { get; set; }
    public bool IsSent { get; set; }
}
