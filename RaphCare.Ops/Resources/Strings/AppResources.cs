using System.Globalization;
using System.Resources;

namespace RaphCare.Ops.Resources.Strings;

public static class AppResources
{
    private static readonly ResourceManager Manager = new(
        "RaphCare.Ops.Resources.Strings.AppResources",
        typeof(AppResources).Assembly);

    private static readonly CultureInfo English = CultureInfo.GetCultureInfo("en");

    public static string T(string name) =>
        Manager.GetString(name, CultureInfo.CurrentUICulture)
        ?? Manager.GetString(name, English)
        ?? string.Empty;

    public static string Format(string name, params object?[] args) =>
        string.Format(CultureInfo.CurrentCulture, T(name), args);
}
