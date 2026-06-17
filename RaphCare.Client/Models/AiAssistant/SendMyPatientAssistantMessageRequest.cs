namespace RaphCare.Client.Models.AiAssistant;

/// <summary>Body for <c>POST api/patient/ai-assistant/chat</c>.</summary>
public sealed class SendMyPatientAssistantMessageRequest
{
    public string Message { get; set; } = string.Empty;
}
