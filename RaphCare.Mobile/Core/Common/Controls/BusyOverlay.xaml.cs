using System.Globalization;
using RaphCare.Mobile.Resources.Strings;

namespace RaphCare.Mobile.Core.Common.Controls;

/// <summary>Branded full-page busy card with a pulsing teal ring and spinner.</summary>
public partial class BusyOverlay : ContentView
{
    private const string PulseAnimationName = "BusyOverlayPulse";
    private bool _animating;

    public static readonly BindableProperty IsBusyProperty = BindableProperty.Create(
        nameof(IsBusy),
        typeof(bool),
        typeof(BusyOverlay),
        false,
        propertyChanged: OnIsBusyChanged);

    public static readonly BindableProperty MessageProperty = BindableProperty.Create(
        nameof(Message),
        typeof(string),
        typeof(BusyOverlay),
        defaultValue: null,
        propertyChanged: OnMessageChanged);

    public BusyOverlay()
    {
        InitializeComponent();
        ApplyDisplayMessage(null);
    }

    public bool IsBusy
    {
        get => (bool)GetValue(IsBusyProperty);
        set => SetValue(IsBusyProperty, value);
    }

    public string? Message
    {
        get => (string?)GetValue(MessageProperty);
        set => SetValue(MessageProperty, value);
    }

    private static void OnIsBusyChanged(BindableObject bindable, object oldValue, object newValue)
    {
        if (bindable is not BusyOverlay overlay)
            return;

        var busy = newValue is true;
        overlay.IsVisible = busy;
        overlay.InputTransparent = !busy;
        if (busy)
            overlay.StartPulse();
        else
            overlay.StopPulse();
    }

    private static void OnMessageChanged(BindableObject bindable, object oldValue, object newValue)
    {
        if (bindable is BusyOverlay overlay)
            overlay.ApplyDisplayMessage(newValue as string);
    }

    private void ApplyDisplayMessage(string? message)
    {
        if (MessageLabel is null)
            return;

        MessageLabel.Text = string.IsNullOrWhiteSpace(message)
            ? AppResources.T("CommonLoadingShort", CultureInfo.CurrentUICulture)
            : message.Trim();
    }

    private void StartPulse()
    {
        if (_animating || PulseRing is null)
            return;

        _animating = true;
        PulseRing.Scale = 1;
        PulseRing.Opacity = 0.55;

        var pulse = new Animation();
        pulse.Add(0, 1, new Animation(v => PulseRing.Scale = v, 1, 1.35, Easing.CubicOut));
        pulse.Add(0, 1, new Animation(v => PulseRing.Opacity = v, 0.55, 0.05, Easing.CubicOut));

        pulse.Commit(
            this,
            PulseAnimationName,
            length: 1200,
            easing: Easing.Linear,
            finished: (_, canceled) =>
            {
                if (canceled || !IsBusy)
                {
                    _animating = false;
                    return;
                }

                _animating = false;
                StartPulse();
            });
    }

    private void StopPulse()
    {
        this.AbortAnimation(PulseAnimationName);
        _animating = false;
        if (PulseRing is null)
            return;

        PulseRing.Scale = 1;
        PulseRing.Opacity = 0.55;
    }
}
