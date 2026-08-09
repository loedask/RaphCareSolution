using System.Globalization;

namespace RaphCare.Web.Services.Localization;

/// <summary>Supported Web UI languages and culture mapping (aligned with mobile).</summary>
public static class WebLanguagePreference
{
    public const string StorageKey = "web_language";

    public static IReadOnlyList<LanguageOption> SupportedLanguages { get; } =
    [
        new LanguageOption("en", "English", "English", CultureInfo.GetCultureInfo("en-US")),
        new LanguageOption("fr", "Français", "French", CultureInfo.GetCultureInfo("fr")),
        new LanguageOption("ln", "Lingála", "Lingala", CultureInfo.GetCultureInfo("ln")),
        new LanguageOption("sw", "Kiswahili", "Swahili", CultureInfo.GetCultureInfo("sw")),
    ];

    public static CultureInfo ResolveCulture(string code) =>
        code switch
        {
            "fr" => CultureInfo.GetCultureInfo("fr"),
            "sw" => CultureInfo.GetCultureInfo("sw"),
            "ln" => CultureInfo.GetCultureInfo("ln"),
            _ => CultureInfo.GetCultureInfo("en-US"),
        };

    public static string NormalizeCode(string? code)
    {
        if (string.IsNullOrWhiteSpace(code))
            return "en";

        var normalized = code.Trim().ToLowerInvariant();
        return SupportedLanguages.Any(l => l.Code == normalized) ? normalized : "en";
    }

    public static void ApplyCulture(string code)
    {
        var culture = ResolveCulture(NormalizeCode(code));
        CultureInfo.DefaultThreadCurrentUICulture = culture;
        CultureInfo.DefaultThreadCurrentCulture = culture;
        CultureInfo.CurrentUICulture = culture;
        CultureInfo.CurrentCulture = culture;
    }
}
