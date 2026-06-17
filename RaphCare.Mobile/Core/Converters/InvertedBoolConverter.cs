using System.Globalization;

namespace RaphCare.Mobile.Core.Converters;

/// <summary>
/// MAUI <see cref="IValueConverter"/> that negates a <see cref="bool"/> binding value.
/// Non-boolean values are returned unchanged. Registered in <c>App.xaml</c> as <c>InvertedBoolConverter</c>.
/// </summary>
public class InvertedBoolConverter : IValueConverter
{
    /// <inheritdoc />
    public object? Convert(object? value, Type targetType, object? parameter, CultureInfo culture) =>
        value is bool b ? !b : value;

    /// <inheritdoc />
    public object? ConvertBack(object? value, Type targetType, object? parameter, CultureInfo culture) =>
        value is bool b ? !b : value;
}
