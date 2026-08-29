using System.Net;
using System.Text.Json;

namespace RaphCare.Client.Services.Base;

/// <summary>
/// Turns API error bodies into short, user-facing messages (never raw JSON for known cases).
/// </summary>
public static class ApiErrorMessages
{
    private static readonly JsonSerializerOptions JsonOptions = new() { PropertyNameCaseInsensitive = true };

    /// <summary>
    /// Maps an HTTP error body to a message suitable for UI. Known tenant errors are rewritten.
    /// </summary>
    public static string FromBody(string? body, HttpStatusCode statusCode)
    {
        var extracted = ExtractMessage(body);
        if (IsMissingClinicHeader(extracted) || IsMissingClinicHeader(body))
            return "Choose your clinic in Profile (My clinic), then try again.";

        if (IsInvalidClinicHeader(extracted) || IsInvalidClinicHeader(body))
            return "Your clinic setting is invalid. Choose your clinic again in Profile.";

        if (!string.IsNullOrWhiteSpace(extracted))
            return extracted!;

        if (!string.IsNullOrWhiteSpace(body) && !LooksLikeJsonObject(body))
            return body.Trim();

        return $"Something went wrong ({(int)statusCode}). Try again.";
    }

    /// <summary>True when the message indicates a missing <c>X-Clinic-Id</c> header.</summary>
    public static bool IsMissingClinicHeader(string? message) =>
        !string.IsNullOrWhiteSpace(message)
        && message.Contains("X-Clinic-Id", StringComparison.OrdinalIgnoreCase)
        && message.Contains("required", StringComparison.OrdinalIgnoreCase);

    /// <summary>True when the message indicates an invalid <c>X-Clinic-Id</c> Guid.</summary>
    public static bool IsInvalidClinicHeader(string? message) =>
        !string.IsNullOrWhiteSpace(message)
        && message.Contains("X-Clinic-Id", StringComparison.OrdinalIgnoreCase)
        && message.Contains("valid Guid", StringComparison.OrdinalIgnoreCase);

    private static string? ExtractMessage(string? body)
    {
        if (string.IsNullOrWhiteSpace(body))
            return null;

        try
        {
            var dto = JsonSerializer.Deserialize<ErrorDto>(body, JsonOptions);
            if (!string.IsNullOrWhiteSpace(dto?.Error))
                return dto.Error.Trim();
            if (!string.IsNullOrWhiteSpace(dto?.Detail))
                return dto.Detail.Trim();
            if (dto?.Errors is { Count: > 0 })
            {
                var messages = dto.Errors
                    .SelectMany(pair => pair.Value)
                    .Where(m => !string.IsNullOrWhiteSpace(m))
                    .ToList();
                if (messages.Count > 0)
                    return string.Join(" ", messages);
            }
        }
        catch (JsonException)
        {
        }

        return null;
    }

    private static bool LooksLikeJsonObject(string body)
    {
        var trimmed = body.TrimStart();
        return trimmed.StartsWith('{') || trimmed.StartsWith('[');
    }

    private sealed class ErrorDto
    {
        public string? Error { get; set; }
        public string? Detail { get; set; }
        public Dictionary<string, string[]>? Errors { get; set; }
    }
}
