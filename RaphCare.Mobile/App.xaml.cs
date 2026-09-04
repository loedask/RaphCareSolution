using RaphCare.Client;
using RaphCare.Mobile.Core.Common.Navigation;
using RaphCare.Mobile.Core.Features.Records;
using RaphCare.Mobile.Core.Infrastructure.Composition;
using RaphCare.Mobile.Resources.Strings;

namespace RaphCare.Mobile;

public partial class App : Application
{
    public App()
    {
        InitializeComponent();
    }

    protected override Window CreateWindow(IActivationState? activationState)
    {
        var shell = MobileServiceHub.GetRequiredService<AppShell>();
        return new Window(shell) { Title = AppResources.WindowTitle };
    }

    protected override void OnAppLinkRequestReceived(Uri uri)
    {
        base.OnAppLinkRequestReceived(uri);
        if (!CollectionQr.TryParsePoster(uri.ToString(), out var clinicId))
            return;

        MobileServiceHub.GetRequiredService<CollectionCheckInStore>().SetClinic(clinicId);
        _ = SafeShellNavigator.GoToAsync("//RecordsPage");
    }
}
