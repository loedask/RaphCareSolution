using RaphCare.Application.Features.PatientAiAssistant.DTOs;

namespace RaphCare.Application.Features.PatientAiAssistant;

/// <summary>
/// Normalizes and windows prior chat turns before Azure OpenAI so cost stays bounded.
/// </summary>
public static class PatientAssistantChatHistoryRules
{
    public const int DefaultMaxPriorMessages = 8;
    public const int AbsoluteMaxPriorMessages = 12;
    public const int MaxContentLength = 2000;

    /// <summary>
    /// Keeps only <c>user</c> / <c>assistant</c> turns with non-empty content, then the last
    /// <paramref name="maxPriorMessages"/> items (clamped to <see cref="AbsoluteMaxPriorMessages"/>).
    /// </summary>
    public static IReadOnlyList<(string Role, string Content)> NormalizeAndWindow(
        IEnumerable<PatientAssistantPriorMessageDto>? priorMessages,
        int maxPriorMessages)
    {
        var limit = Math.Clamp(maxPriorMessages, 0, AbsoluteMaxPriorMessages);
        if (limit == 0 || priorMessages is null)
            return [];

        var normalized = new List<(string Role, string Content)>();
        foreach (var item in priorMessages)
        {
            if (item is null)
                continue;

            if (!TryNormalizeRole(item.Role, out var role))
                continue;

            var content = NormalizeContent(item.Content);
            if (content is null)
                continue;

            normalized.Add((role, content));
        }

        if (normalized.Count <= limit)
            return normalized;

        return normalized.Skip(normalized.Count - limit).ToList();
    }

    public static bool TryNormalizeRole(string? role, out string normalized)
    {
        normalized = string.Empty;
        if (string.IsNullOrWhiteSpace(role))
            return false;

        var trimmed = role.Trim();
        if (trimmed.Equals("user", StringComparison.OrdinalIgnoreCase))
        {
            normalized = "user";
            return true;
        }

        if (trimmed.Equals("assistant", StringComparison.OrdinalIgnoreCase))
        {
            normalized = "assistant";
            return true;
        }

        return false;
    }

    public static string? NormalizeContent(string? content)
    {
        if (string.IsNullOrWhiteSpace(content))
            return null;

        var trimmed = content.Trim();
        if (trimmed.Contains('\0', StringComparison.Ordinal))
            return null;

        if (trimmed.Length > MaxContentLength)
            trimmed = trimmed[..MaxContentLength];

        return trimmed;
    }
}
