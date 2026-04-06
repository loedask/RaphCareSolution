namespace RaphCare.Application.Features.PatientAiAssistant.DTOs;

/// <summary>Response from <c>POST api/patient/ai-assistant/chat</c>.</summary>
public sealed class PatientAssistantReplyDto
{
    public string Reply { get; set; } = string.Empty;
    public string MedicalDisclaimer { get; set; } = string.Empty;
}
