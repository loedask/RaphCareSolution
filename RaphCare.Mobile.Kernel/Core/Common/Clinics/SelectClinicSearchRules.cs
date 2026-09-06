namespace RaphCare.Mobile.Core.Common.Clinics;

/// <summary>
/// Rules for the patient active-clinic picker so directory search is never mistaken for membership.
/// </summary>
public static class SelectClinicSearchRules
{
    /// <summary>
    /// Returns true when the user typed a real search. Empty or whitespace must not load the full clinic directory.
    /// </summary>
    public static bool TryNormalizeQuery(string? searchText, out string query)
    {
        query = (searchText ?? string.Empty).Trim();
        return query.Length > 0;
    }
}
