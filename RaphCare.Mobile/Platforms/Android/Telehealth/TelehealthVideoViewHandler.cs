using Android.Views;
using Microsoft.Maui.Handlers;
using RaphCare.Mobile.Core.Features.CareTelehealth.Views;

namespace RaphCare.Mobile.Platforms.Android.Telehealth;

public sealed class TelehealthVideoViewHandler : ViewHandler<TelehealthVideoView, TextureView>
{
    public TelehealthVideoViewHandler() : base(ViewHandler.ViewMapper)
    {
    }

    protected override TextureView CreatePlatformView() =>
        new TextureView(Context ?? throw new InvalidOperationException("Android Context is required."));

    protected override void ConnectHandler(TextureView platformView)
    {
    }

    protected override void DisconnectHandler(TextureView platformView)
    {
    }
}
