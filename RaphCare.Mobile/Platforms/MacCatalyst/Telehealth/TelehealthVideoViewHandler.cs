using Microsoft.Maui.Handlers;
using RaphCare.Mobile.Core.Features.CareTelehealth.Views;
using UIKit;

namespace RaphCare.Mobile.Platforms.MacCatalyst.Telehealth;

public sealed class TelehealthVideoViewHandler : ViewHandler<TelehealthVideoView, UIView>
{
    public TelehealthVideoViewHandler() : base(ViewHandler.ViewMapper)
    {
    }

    protected override UIView CreatePlatformView() =>
        new UIView { BackgroundColor = UIColor.DarkGray };
}
