namespace RaphCare.Client.Models.AiAssistant;

/// <summary>Response from <c>POST api/patient/ai-assistant/chat</c>.</summary>
public sealed class PatientAssistantReplyViewModel
{
    public string Reply { get; set; } = string.Empty;
    public string MedicalDisclaimer { get; set; } = string.Empty;
}
