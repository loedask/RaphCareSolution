namespace RaphCare.Client.Models.Telehealth;

public class TelehealthChatMessageViewModel
{
    public Guid Id { get; init; }
    public string Message { get; init; } = string.Empty;
    public DateTime SentAt { get; init; }
    public bool IsMine { get; init; }
    public bool IsSystemMessage { get; init; }
}
