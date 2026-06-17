using RaphCare.Application.Common.DTOs;

namespace RaphCare.Application.Features.Communication.DTOs;

public class MessageDto : BaseDto
{
    public Guid ClinicId { get; set; }
    public string RecipientUserId { get; set; } = string.Empty;
    public string Channel { get; set; } = string.Empty;
    public string? Subject { get; set; }
    public string Body { get; set; } = string.Empty;
    public DateTime? SentAt { get; set; }
    public bool IsSent { get; set; }
}
