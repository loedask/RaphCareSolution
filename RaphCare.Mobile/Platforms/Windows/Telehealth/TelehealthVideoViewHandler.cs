using Microsoft.Maui.Handlers;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Media;
using RaphCare.Mobile.Core.Features.CareTelehealth.Views;

namespace RaphCare.Mobile.Platforms.Windows.Telehealth;

public sealed class TelehealthVideoViewHandler : ViewHandler<TelehealthVideoView, Microsoft.UI.Xaml.Controls.Border>
{
    public TelehealthVideoViewHandler() : base(ViewHandler.ViewMapper)
    {
    }

    protected override Microsoft.UI.Xaml.Controls.Border CreatePlatformView() => new()
    {
        Background = new Microsoft.UI.Xaml.Media.SolidColorBrush(Microsoft.UI.Colors.DimGray)
    };
}
