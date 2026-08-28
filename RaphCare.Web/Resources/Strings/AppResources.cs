using System.Globalization;
using System.Resources;

namespace RaphCare.Web.Resources.Strings;

/// <summary>
/// Localized strings from <c>Resources/Strings/AppResources.resx</c>.
/// Add culture-specific files (e.g. AppResources.fr.resx) to extend languages.
/// </summary>
public static class AppResources
{
    private static readonly ResourceManager Manager = new(
        "RaphCare.Web.Resources.Strings.AppResources",
        typeof(AppResources).Assembly);

    private static readonly CultureInfo English = CultureInfo.GetCultureInfo("en");

    /// <summary>Resolves a string for the current UI culture, then falls back to English.</summary>
    public static string T(string name) =>
        T(name, CultureInfo.CurrentUICulture);

    /// <summary>Resolves a string for a specific culture.</summary>
    public static string T(string name, CultureInfo culture) =>
        Manager.GetString(name, culture)
        ?? Manager.GetString(name, English)
        ?? string.Empty;

    /// <summary>Formats a localized string with the current culture.</summary>
    public static string Format(string name, params object?[] args) =>
        string.Format(CultureInfo.CurrentCulture, T(name, CultureInfo.CurrentUICulture), args);
}
