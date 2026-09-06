namespace RaphCare.Mobile.Core.Common.Clinics;

/// <summary>Picks the Active clinic linked-list error text (API detail when present, else fallback).</summary>
public static class LinkedClinicLoadFailureMessage
{
    /// <summary>
    /// Prefer a non-empty API error so the phone shows connection or auth detail instead of a generic line.
    /// </summary>
    public static string Resolve(string? apiError, string fallback)
    {
        if (string.IsNullOrWhiteSpace(fallback))
            throw new ArgumentException("Fallback message is required.", nameof(fallback));

        return string.IsNullOrWhiteSpace(apiError) ? fallback : apiError.Trim();
    }
}
