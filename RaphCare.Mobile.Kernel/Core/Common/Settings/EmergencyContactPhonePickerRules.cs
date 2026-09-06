namespace RaphCare.Mobile.Core.Common.Settings;

/// <summary>
/// Pure mapping from a device address-book pick into emergency-contact form fields.
/// Keeps phone-picker behavior testable without MAUI.
/// </summary>
public static class EmergencyContactPhonePickerRules
{
    /// <summary>Result of mapping a picked phone contact into form values.</summary>
    /// <param name="Name">Display name for the form (may be empty).</param>
    /// <param name="Phone">Chosen phone number (empty when none).</param>
    /// <param name="HasPhone">True when at least one non-blank phone number was available.</param>
    public readonly record struct MappedContact(string Name, string Phone, bool HasPhone);

    /// <summary>
    /// Maps a display name and phone-number list into form fields.
    /// Uses the first non-blank phone. Relationship is never inferred.
    /// </summary>
    public static MappedContact MapFromPhoneContact(string? displayName, IEnumerable<string?>? phoneNumbers)
    {
        var name = string.IsNullOrWhiteSpace(displayName) ? string.Empty : displayName.Trim();
        var phone = FirstNonBlankPhone(phoneNumbers);
        return new MappedContact(name, phone ?? string.Empty, phone is not null);
    }

    /// <summary>
    /// When the contact has several phone numbers, returns non-blank trimmed numbers for a chooser sheet.
    /// </summary>
    public static IReadOnlyList<string> DistinctPhoneChoices(IEnumerable<string?>? phoneNumbers)
    {
        if (phoneNumbers is null)
            return [];

        var list = new List<string>();
        var seen = new HashSet<string>(StringComparer.Ordinal);
        foreach (var raw in phoneNumbers)
        {
            if (string.IsNullOrWhiteSpace(raw))
                continue;
            var trimmed = raw.Trim();
            if (seen.Add(trimmed))
                list.Add(trimmed);
        }

        return list;
    }

    private static string? FirstNonBlankPhone(IEnumerable<string?>? phoneNumbers)
    {
        if (phoneNumbers is null)
            return null;

        foreach (var raw in phoneNumbers)
        {
            if (!string.IsNullOrWhiteSpace(raw))
                return raw.Trim();
        }

        return null;
    }
}
