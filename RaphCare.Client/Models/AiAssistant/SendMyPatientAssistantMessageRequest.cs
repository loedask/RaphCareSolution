namespace RaphCare.Client.Models.AiAssistant;

/// <summary>Body for <c>POST api/patient/ai-assistant/chat</c>.</summary>
public sealed class SendMyPatientAssistantMessageRequest
{
    public string Message { get; set; } = string.Empty;

    /// <summary>Optional earlier turns from the phone session (server windows again).</summary>
    public List<PatientAssistantPriorMessage>? PriorMessages { get; set; }
}
