using System.Collections.ObjectModel;
using System.Globalization;
using Microsoft.Maui.Storage;
using RaphCare.Mobile.Core.Features.Auth.ViewModels;

namespace RaphCare.Mobile.Core.Common.Services.Localization;

/// <summary>Shared language preference used on landing and profile language screens.</summary>
public static class AppLanguagePreference
{
    public const string PreferenceKey = "auth_language";

    public static IReadOnlyList<LanguageOption> SupportedLanguages { get; } =
    [
        new LanguageOption("en", "English", "English", CultureInfo.GetCultureInfo("en-US")),
        new LanguageOption("fr", "Français", "French", CultureInfo.GetCultureInfo("fr")),
        new LanguageOption("ln", "Lingála", "Lingala", CultureInfo.GetCultureInfo("ln")),
        new LanguageOption("sw", "Kiswahili", "Swahili", CultureInfo.GetCultureInfo("sw")),
    ];

    public static string CurrentCode => Preferences.Get(PreferenceKey, "en");

    public static string CurrentNativeName =>
        SupportedLanguages.FirstOrDefault(l => l.Code == CurrentCode)?.Native ?? "English";

    public static void Apply(string code)
    {
        Preferences.Set(PreferenceKey, code);
        var culture = ResolveCulture(code);
        CultureInfo.DefaultThreadCurrentUICulture = culture;
        CultureInfo.CurrentUICulture = culture;
    }

    public static CultureInfo ResolveCulture(string code) =>
        code switch
        {
            "fr" => CultureInfo.GetCultureInfo("fr"),
            "sw" => CultureInfo.GetCultureInfo("sw"),
            "ln" => CultureInfo.GetCultureInfo("ln"),
            _ => CultureInfo.GetCultureInfo("en-US"),
        };

    public static ObservableCollection<LanguageOption> CreateSelectableList()
    {
        var current = CurrentCode;
        var list = new ObservableCollection<LanguageOption>();
        foreach (var lang in SupportedLanguages)
        {
            lang.IsCurrent = lang.Code == current;
            list.Add(lang);
        }

        return list;
    }

    public static void MarkSelection(ObservableCollection<LanguageOption> languages, string selectedCode)
    {
        foreach (var l in languages)
            l.IsCurrent = l.Code == selectedCode;
    }
}
