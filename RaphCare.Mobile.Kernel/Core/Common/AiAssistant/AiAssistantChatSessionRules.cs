namespace RaphCare.Mobile.Core.Common.AiAssistant;

/// <summary>
/// Pure rules for the patient AI assistant conversation (normalize send text, greeting seed, prior window).
/// Keeps chat-thread behavior testable without MAUI.
/// </summary>
public static class AiAssistantChatSessionRules
{
    /// <summary>Matches API validator max length for <c>POST api/patient/ai-assistant/chat</c>.</summary>
    public const int MaxMessageLength = 2000;

    /// <summary>Matches API default <c>PatientAssistant:MaxPriorMessages</c> (cost window).</summary>
    public const int DefaultMaxPriorMessages = 8;

    /// <summary>
    /// Trims draft text and caps length. Returns <see langword="null"/> when there is nothing to send.
    /// </summary>
    public static string? TryNormalizeOutgoing(string? draft)
    {
        if (string.IsNullOrWhiteSpace(draft))
            return null;

        var trimmed = draft.Trim();
        if (trimmed.Length > MaxMessageLength)
            trimmed = trimmed[..MaxMessageLength];

        return trimmed;
    }

    /// <summary>
    /// True when the conversation should start with a single assistant greeting and nothing else.
    /// </summary>
    public static bool NeedsGreetingSeed(int messageCount) => messageCount <= 0;

    /// <summary>
    /// Picks the last <paramref name="maxPriorMessages"/> non-empty bubbles to send as model context
    /// (does not include the message about to be sent).
    /// </summary>
    public static IReadOnlyList<(bool IsFromUser, string Text)> SelectPriorForRequest(
        IEnumerable<(bool IsFromUser, string Text)> existingMessages,
        int maxPriorMessages = DefaultMaxPriorMessages)
    {
        ArgumentNullException.ThrowIfNull(existingMessages);

        var limit = Math.Clamp(maxPriorMessages, 0, 12);
        if (limit == 0)
            return [];

        var list = existingMessages
            .Where(m => !string.IsNullOrWhiteSpace(m.Text))
            .Select(m => (m.IsFromUser, Text: m.Text.Trim()))
            .ToList();

        if (list.Count <= limit)
            return list;

        return list.Skip(list.Count - limit).ToList();
    }
}
