namespace RaphCare.Domain.Identity;

/// <summary>
/// Well-known Staging demo emails. Sign-in skips email verification for these addresses
/// so partners can use accounts that do not have real mailboxes.
/// </summary>
public static class DemoPackAccounts
{
    public const string AdminEmail = "demo.admin@raphcare.com";
    public const string DoctorEmail = "demo.doctor@raphcare.com";
    public const string PharmacistEmail = "demo.pharmacy@raphcare.com";
    public const string LabEmail = "demo.lab@raphcare.com";
    public const string PatientEmail = "demo.patient@raphcare.com";

    /// <summary>Earlier staging seeds used this domain. Still accepted for sign-in skip and seeder migration.</summary>
    public const string LegacyDemoDomain = "@raphcare.demo";

    private static readonly string[] CanonicalEmails =
    [
        AdminEmail,
        DoctorEmail,
        PharmacistEmail,
        LabEmail,
        PatientEmail
    ];

    public static bool IsDemoEmail(string? email)
    {
        if (string.IsNullOrWhiteSpace(email))
            return false;

        var normalized = email.Trim().ToLowerInvariant();
        if (CanonicalEmails.Contains(normalized, StringComparer.Ordinal))
            return true;

        // Legacy seed domain (demo.*@raphcare.demo).
        if (!normalized.EndsWith(LegacyDemoDomain, StringComparison.Ordinal))
            return false;

        var local = normalized[..^LegacyDemoDomain.Length];
        return CanonicalEmails.Any(e =>
            e.StartsWith(local + "@", StringComparison.Ordinal));
    }

    /// <summary>Maps a canonical demo email to the legacy <c>@raphcare.demo</c> address used by older seeds.</summary>
    public static string ToLegacyDemoEmail(string canonicalEmail) =>
        canonicalEmail.Trim().ToLowerInvariant().Replace("@raphcare.com", LegacyDemoDomain, StringComparison.Ordinal);
}
