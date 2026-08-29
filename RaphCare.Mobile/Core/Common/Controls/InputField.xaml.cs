using System.Globalization;
using RaphCare.Mobile.Resources.Strings;

namespace RaphCare.Mobile.Core.Common.Controls;

public partial class InputField : VerticalStackLayout
{
    public static readonly BindableProperty LabelTextProperty =
        BindableProperty.Create(nameof(LabelText), typeof(string), typeof(InputField), string.Empty);

    public static readonly BindableProperty PlaceholderProperty =
        BindableProperty.Create(nameof(Placeholder), typeof(string), typeof(InputField), string.Empty);

    public static readonly BindableProperty TextProperty =
        BindableProperty.Create(nameof(Text), typeof(string), typeof(InputField), default(string), BindingMode.TwoWay);

    public static readonly BindableProperty ErrorTextProperty =
        BindableProperty.Create(nameof(ErrorText), typeof(string), typeof(InputField), string.Empty);

    public static readonly BindableProperty IsPasswordProperty =
        BindableProperty.Create(
            nameof(IsPassword),
            typeof(bool),
            typeof(InputField),
            false,
            propertyChanged: OnPasswordPresentationChanged);

    public static readonly BindableProperty IsPasswordVisibleProperty =
        BindableProperty.Create(
            nameof(IsPasswordVisible),
            typeof(bool),
            typeof(InputField),
            false,
            propertyChanged: OnPasswordPresentationChanged);

    public static readonly BindableProperty KeyboardProperty =
        BindableProperty.Create(nameof(Keyboard), typeof(Keyboard), typeof(InputField), Keyboard.Default);

    public string LabelText { get => (string)GetValue(LabelTextProperty); set => SetValue(LabelTextProperty, value); }
    public string Placeholder { get => (string)GetValue(PlaceholderProperty); set => SetValue(PlaceholderProperty, value); }
    public string? Text { get => (string?)GetValue(TextProperty); set => SetValue(TextProperty, value); }
    public string ErrorText { get => (string)GetValue(ErrorTextProperty); set => SetValue(ErrorTextProperty, value); }
    public bool IsPassword { get => (bool)GetValue(IsPasswordProperty); set => SetValue(IsPasswordProperty, value); }
    public bool IsPasswordVisible { get => (bool)GetValue(IsPasswordVisibleProperty); set => SetValue(IsPasswordVisibleProperty, value); }
    public Keyboard Keyboard { get => (Keyboard)GetValue(KeyboardProperty); set => SetValue(KeyboardProperty, value); }

    public InputField()
    {
        InitializeComponent();
        Loaded += (_, _) => UpdatePasswordPresentation();
    }

    private void OnPasswordVisibilityClicked(object? sender, EventArgs e)
    {
        IsPasswordVisible = !IsPasswordVisible;
    }

    private static void OnPasswordPresentationChanged(BindableObject bindable, object oldValue, object newValue)
    {
        if (bindable is InputField field)
            field.UpdatePasswordPresentation();
    }

    private void UpdatePasswordPresentation()
    {
        if (InputEntry is null || PasswordVisibilityButton is null)
            return;

        InputEntry.IsPassword = IsPassword && !IsPasswordVisible;
        PasswordVisibilityButton.IsVisible = IsPassword;

        var label = IsPasswordVisible
            ? AppResources.T("AuthHidePassword", CultureInfo.CurrentUICulture)
            : AppResources.T("AuthShowPassword", CultureInfo.CurrentUICulture);
        PasswordVisibilityButton.Text = label;
        SemanticProperties.SetDescription(PasswordVisibilityButton, label);
    }
}
