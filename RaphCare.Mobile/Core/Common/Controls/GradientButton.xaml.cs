namespace RaphCare.Mobile.Core.Common.Controls;

/// <summary>Primary teal gradient CTA. Dimmed and disabled while <see cref="IsBusy"/>.</summary>
public partial class GradientButton : Button
{
    public static readonly BindableProperty IsBusyProperty = BindableProperty.Create(
        nameof(IsBusy),
        typeof(bool),
        typeof(GradientButton),
        false,
        propertyChanged: OnIsBusyChanged);

    public GradientButton()
    {
        InitializeComponent();
    }

    public bool IsBusy
    {
        get => (bool)GetValue(IsBusyProperty);
        set => SetValue(IsBusyProperty, value);
    }

    private static void OnIsBusyChanged(BindableObject bindable, object oldValue, object newValue)
    {
        if (bindable is not GradientButton button)
            return;

        var busy = newValue is true;
        button.Opacity = busy ? 0.72 : 1;
        button.IsEnabled = !busy;
    }
}
