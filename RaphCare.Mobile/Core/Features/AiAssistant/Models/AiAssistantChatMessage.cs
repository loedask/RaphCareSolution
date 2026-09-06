namespace RaphCare.Mobile.Core.Features.AiAssistant.Models;

/// <summary>One bubble in the patient AI assistant thread.</summary>
public sealed class AiAssistantChatMessage
{
    public AiAssistantChatMessage(bool isFromUser, string text)
    {
        IsFromUser = isFromUser;
        Text = text ?? string.Empty;
    }

    public bool IsFromUser { get; }

    public string Text { get; }
}
