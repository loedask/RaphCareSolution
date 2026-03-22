using System.Globalization;
using System.Resources;

namespace RaphCare.Mobile.Resources.Strings;

/// <summary>
/// Localized strings from <c>Resources/Strings/AppResources.resx</c>. Add culture-specific files (e.g. AppResources.es.resx) to extend languages.
/// </summary>
public static class AppResources
{
    private static readonly ResourceManager Manager = new(
        "RaphCare.Mobile.Resources.Strings.AppResources",
        typeof(AppResources).Assembly);

    public static string WindowTitle => Get(nameof(WindowTitle), "RaphCare");
    public static string HomeWelcome => Get(nameof(HomeWelcome), "Welcome to RaphCare");
    public static string HomeSignedIn => Get(nameof(HomeSignedIn), "You're signed in.");
    public static string OpenBlazorSample => Get(nameof(OpenBlazorSample), "Open Blazor sample UI");

    private static string Get(string name, string fallback) =>
        Manager.GetString(name, CultureInfo.CurrentUICulture) ?? fallback;
}
