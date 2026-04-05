using System.Globalization;
using System.Resources;

namespace RaphCare.Mobile.Resources.Strings;

/// <summary>
/// Localized strings from <c>Resources/Strings/AppResources.resx</c>. Add culture-specific files (e.g. AppResources.fr.resx) to extend languages.
/// </summary>
public static class AppResources
{
    private static readonly ResourceManager Manager = new(
        "RaphCare.Mobile.Resources.Strings.AppResources",
        typeof(AppResources).Assembly);

    private static readonly CultureInfo English = CultureInfo.GetCultureInfo("en");

    /// <summary>Resolves a string for the current UI culture, then falls back to English.</summary>
    public static string T(string name) =>
        T(name, CultureInfo.CurrentUICulture);

    /// <summary>Resolves a string for a specific culture (used before thread UI culture is switched).</summary>
    public static string T(string name, CultureInfo culture) =>
        Manager.GetString(name, culture)
        ?? Manager.GetString(name, English)
        ?? string.Empty;

    public static string WindowTitle => Get(nameof(WindowTitle), "RaphCare");
    public static string HomeWelcome => Get(nameof(HomeWelcome), "Welcome to RaphCare");
    public static string HomeSignedIn => Get(nameof(HomeSignedIn), "You're signed in.");
    public static string OpenBlazorSample => Get(nameof(OpenBlazorSample), "Open Blazor sample UI");
    public static string HomeHubSectionCare => Get(nameof(HomeHubSectionCare), "Your care");
    public static string HomeHubSectionMore => Get(nameof(HomeHubSectionMore), "More");
    public static string HomeHubHealthRecords => Get(nameof(HomeHubHealthRecords), "Health records");
    public static string HomeHubAppointments => Get(nameof(HomeHubAppointments), "Appointments");
    public static string HomeHubInsurance => Get(nameof(HomeHubInsurance), "Insurance");
    public static string HomeHubCareTelehealth => Get(nameof(HomeHubCareTelehealth), "Care & telehealth");
    public static string HomeHubDevices => Get(nameof(HomeHubDevices), "Devices & wearables");
    public static string HomeHubBilling => Get(nameof(HomeHubBilling), "Billing & plans");
    public static string HomeHubMentalHealth => Get(nameof(HomeHubMentalHealth), "Mental health");
    public static string HomeHubFamily => Get(nameof(HomeHubFamily), "Family members");
    public static string HomeHubAiAssistant => Get(nameof(HomeHubAiAssistant), "AI assistant");
    public static string HomeHubNotifications => Get(nameof(HomeHubNotifications), "Notifications");
    public static string HomeHubSettings => Get(nameof(HomeHubSettings), "Profile");
    public static string HomeHubSamples => Get(nameof(HomeHubSamples), "Developer samples");

    private static string Get(string name, string fallback) =>
        Manager.GetString(name, CultureInfo.CurrentUICulture) ?? fallback;

}
