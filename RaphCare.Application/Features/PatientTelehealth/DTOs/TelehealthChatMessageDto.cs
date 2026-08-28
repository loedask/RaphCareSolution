namespace RaphCare.Application.Features.PatientTelehealth.DTOs;

public class TelehealthChatMessageDto
{
    public Guid Id { get; set; }
    public string Message { get; set; } = string.Empty;
    public DateTime SentAt { get; set; }
    public bool IsMine { get; set; }
    public bool IsSystemMessage { get; set; }
}
