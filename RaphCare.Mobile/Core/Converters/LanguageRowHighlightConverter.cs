using System.Globalization;

namespace RaphCare.Mobile.Core.Converters;

/// <summary>Background for the language picker row when the bound value is true (current language).</summary>
public sealed class LanguageRowHighlightConverter : IValueConverter
{
    public object? Convert(object? value, Type targetType, object? parameter, CultureInfo culture) =>
        value is true ? Color.FromArgb("#1A1B9BBB") : Colors.Transparent;

    public object? ConvertBack(object? value, Type targetType, object? parameter, CultureInfo culture) =>
        throw new NotSupportedException();
}
