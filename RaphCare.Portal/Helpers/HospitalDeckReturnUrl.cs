namespace RaphCare.Portal.Helpers;

/// <summary>
/// Builds and validates same-clinic return paths for hospital-deck back links.
/// </summary>
public static class HospitalDeckReturnUrl
{
    /// <summary>
    /// Hospital profile tabs that live on their own routes. Back links return to the
    /// hospital page with this tab so the matching rail item stays highlighted.
    /// </summary>
    public static bool IsLinkedSectionTab(string? tab) =>
        tab is "inpatient" or "collection" or "casualty" or "consult" or "theatre" or "referrals" or "roster" or "day-sheet";

    public static string ForClinicTab(Guid clinicId, string tab) =>
        $"/admin/hospitals/{clinicId}?tab={Uri.EscapeDataString(tab)}";

    public static string ForLinkedSectionReturn(Guid clinicId, string section) =>
        ForClinicTab(clinicId, section);

    public static string ForClinicPage(Guid clinicId, string page, string? tab = null)
    {
        var path = $"/admin/hospitals/{clinicId}/{page.Trim('/')}";
        return string.IsNullOrWhiteSpace(tab)
            ? path
            : $"{path}?tab={Uri.EscapeDataString(tab)}";
    }

    public static string PatientHref(Guid clinicId, Guid patientId, string returnPath) =>
        $"/admin/hospitals/{clinicId}/patients/{patientId}?return={Uri.EscapeDataString(returnPath)}";

    public static string Resolve(Guid clinicId, string? returnQuery, string fallback)
    {
        if (TryValidate(clinicId, returnQuery, out var safe))
            return safe;

        return fallback;
    }

    public static bool TryValidate(Guid clinicId, string? returnQuery, out string path)
    {
        path = string.Empty;
        if (string.IsNullOrWhiteSpace(returnQuery))
            return false;

        var candidate = Uri.UnescapeDataString(returnQuery.Trim());
        if (candidate.Length == 0 || candidate.Length > 260)
            return false;

        if (candidate.Contains("://", StringComparison.Ordinal)
            || candidate.StartsWith("//", StringComparison.Ordinal)
            || candidate.Contains('\\', StringComparison.Ordinal)
            || candidate.Contains('\r', StringComparison.Ordinal)
            || candidate.Contains('\n', StringComparison.Ordinal))
        {
            return false;
        }

        if (!candidate.StartsWith('/'))
            candidate = "/" + candidate;

        var prefix = $"/admin/hospitals/{clinicId:D}";
        if (!candidate.StartsWith(prefix, StringComparison.OrdinalIgnoreCase))
            return false;

        // Block path traversal after the clinic root.
        var remainder = candidate[prefix.Length..];
        if (remainder.Contains("..", StringComparison.Ordinal))
            return false;

        path = candidate;
        return true;
    }
}
