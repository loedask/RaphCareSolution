using System.Globalization;

namespace RaphCare.Mobile.Core.Converters;

/// <summary>
/// MAUI <see cref="IValueConverter"/> that returns <c>true</c> when the bound value is a non-empty string.
/// Use for <see cref="VisualElement.IsEnabled"/>, visibility, or command <c>CanExecute</c> via bindings.
/// Registered in <c>App.xaml</c> as <c>StringNotEmptyConverter</c>.
/// </summary>
public class StringNotEmptyConverter : IValueConverter
{
    /// <inheritdoc />
    public object? Convert(object? value, Type targetType, object? parameter, CultureInfo culture) =>
        value is string s && !string.IsNullOrEmpty(s);

    /// <summary>
    /// Not supported; string cannot be recovered unambiguously from a boolean.
    /// </summary>
    public object? ConvertBack(object? value, Type targetType, object? parameter, CultureInfo culture) =>
        throw new NotImplementedException();
}
