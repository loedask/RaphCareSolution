namespace RaphCare.Application.Features.PatientAiAssistant.DTOs;

/// <summary>One earlier chat turn sent with a new patient assistant message (no PHI injection).</summary>
public sealed class PatientAssistantPriorMessageDto
{
    /// <summary><c>user</c> or <c>assistant</c>.</summary>
    public string Role { get; set; } = string.Empty;

    public string Content { get; set; } = string.Empty;
}
