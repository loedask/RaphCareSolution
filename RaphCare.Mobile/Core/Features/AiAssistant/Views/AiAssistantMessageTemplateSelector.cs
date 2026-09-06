using RaphCare.Mobile.Core.Features.AiAssistant.Models;

namespace RaphCare.Mobile.Core.Features.AiAssistant.Views;

/// <summary>Picks user (right) vs assistant (left) bubble templates.</summary>
public sealed class AiAssistantMessageTemplateSelector : DataTemplateSelector
{
    public DataTemplate? UserTemplate { get; set; }

    public DataTemplate? AssistantTemplate { get; set; }

    protected override DataTemplate OnSelectTemplate(object item, BindableObject container)
    {
        if (item is AiAssistantChatMessage { IsFromUser: true })
            return UserTemplate ?? AssistantTemplate
                ?? throw new InvalidOperationException("UserTemplate is not set.");

        return AssistantTemplate
            ?? throw new InvalidOperationException("AssistantTemplate is not set.");
    }
}
