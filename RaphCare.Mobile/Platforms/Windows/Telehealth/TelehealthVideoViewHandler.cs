using Microsoft.Maui.Handlers;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Media;
using RaphCare.Mobile.Core.Features.CareTelehealth.Views;

namespace RaphCare.Mobile.Platforms.Windows.Telehealth;

public sealed class TelehealthVideoViewHandler : ViewHandler<TelehealthVideoView, Border>
{
    public TelehealthVideoViewHandler() : base(ViewHandler.ViewMapper)
    {
    }

    protected override Border CreatePlatformView() => new()
    {
        Background = new SolidColorBrush(Microsoft.UI.Colors.DimGray)
    };
}
