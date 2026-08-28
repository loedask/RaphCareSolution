using System.Globalization;

namespace RaphCare.Web.Services.Localization;

/// <summary>Selectable UI language for the Web portal.</summary>
public sealed class LanguageOption
{
    public LanguageOption(string code, string native, string english, CultureInfo culture)
    {
        Code = code;
        Native = native;
        English = english;
        Culture = culture;
    }

    public string Code { get; }
    public string Native { get; }
    public string English { get; }
    public CultureInfo Culture { get; }
    public bool IsCurrent { get; set; }
}
