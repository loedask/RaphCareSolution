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
            return "This action needs a hospital selected. Open the hospital in Portal, or pick a hospital on this page if you are in Ops.";

        if (IsInvalidClinicHeader(extracted) || IsInvalidClinicHeader(body))
            return "The hospital setting is invalid. Pick the hospital again and try again.";

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

    /// <summary>
    /// True for missing/invalid clinic header errors, including rewritten user-facing messages.
    /// </summary>
    public static bool IsClinicSelectionRequired(string? message) =>
        IsMissingClinicHeader(message)
        || IsInvalidClinicHeader(message)
        || (!string.IsNullOrWhiteSpace(message)
            && message.Contains("Choose your clinic", StringComparison.OrdinalIgnoreCase))
        || (!string.IsNullOrWhiteSpace(message)
            && message.Contains("clinic setting is invalid", StringComparison.OrdinalIgnoreCase))
        || (!string.IsNullOrWhiteSpace(message)
            && message.Contains("needs a hospital selected", StringComparison.OrdinalIgnoreCase))
        || (!string.IsNullOrWhiteSpace(message)
            && message.Contains("hospital setting is invalid", StringComparison.OrdinalIgnoreCase));

    private static string? ExtractMessage(string? body)
    {
        if (string.IsNullOrWhiteSpace(body))
            return null;

        try
        {
            var dto = JsonSerializer.Deserialize<ErrorDto>(body, JsonOptions);
            if (!string.IsNullOrWhiteSpace(dto?.Error))
                return dto.Error.Trim();

            // Prefer field errors over ProblemDetails.detail ("One or more validation failures…").
            if (dto?.Errors is { Count: > 0 })
            {
                var messages = dto.Errors
                    .SelectMany(pair => pair.Value ?? Array.Empty<string>())
                    .Where(m => !string.IsNullOrWhiteSpace(m))
                    .Select(m => m.Trim())
                    .Distinct(StringComparer.Ordinal)
                    .ToList();
                if (messages.Count > 0)
                    return string.Join(" ", messages);
            }

            if (!string.IsNullOrWhiteSpace(dto?.Detail)
                && !IsGenericValidationDetail(dto.Detail))
                return dto.Detail.Trim();
        }
        catch (JsonException)
        {
        }

        return null;
    }

    private static bool IsGenericValidationDetail(string detail) =>
        detail.Contains("One or more validation failures", StringComparison.OrdinalIgnoreCase);

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
