namespace RaphCare.Mobile.Core.Common.Navigation;

/// <summary>
/// Absolute Shell paths (<c>//Route</c>) only work for destinations that exist as
/// <c>ShellContent</c> / flyout items. Push-only registered routes crash on Android when targeted with <c>//</c>.
/// </summary>
public static class AbsoluteShellRouteRules
{
    private static readonly HashSet<string> AbsoluteSafeRoots = new(StringComparer.Ordinal)
    {
        "LandingPage",
        "HomePage",
        "RecordsPage",
        "AppointmentsPage",
        "InsurancePage",
        "SettingsPage",
    };

    /// <summary>
    /// Returns the first path segment of a Shell URI (strips leading <c>//</c> and query).
    /// </summary>
    public static string RootSegment(string route)
    {
        ArgumentNullException.ThrowIfNull(route);
        var trimmed = route.Trim();
        if (trimmed.StartsWith("//", StringComparison.Ordinal))
            trimmed = trimmed[2..];
        else if (trimmed.StartsWith('/'))
            trimmed = trimmed[1..];

        var slash = trimmed.IndexOf('/');
        if (slash >= 0)
            trimmed = trimmed[..slash];
        var query = trimmed.IndexOf('?');
        if (query >= 0)
            trimmed = trimmed[..query];
        return trimmed;
    }

    /// <summary>True when <paramref name="route"/> may safely use absolute <c>//</c> navigation.</summary>
    public static bool IsSafeAbsoluteTarget(string route) =>
        AbsoluteSafeRoots.Contains(RootSegment(route));
}
