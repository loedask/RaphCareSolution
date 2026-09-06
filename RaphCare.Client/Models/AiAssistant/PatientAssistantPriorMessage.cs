namespace RaphCare.Client.Models.AiAssistant;

/// <summary>Earlier chat turn for <c>POST api/patient/ai-assistant/chat</c>.</summary>
public sealed class PatientAssistantPriorMessage
{
    /// <summary><c>user</c> or <c>assistant</c>.</summary>
    public string Role { get; set; } = string.Empty;

    public string Content { get; set; } = string.Empty;
}
