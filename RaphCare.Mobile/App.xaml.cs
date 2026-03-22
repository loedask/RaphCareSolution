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
}
