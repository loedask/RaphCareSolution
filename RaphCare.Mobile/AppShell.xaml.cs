using RaphCare.Mobile.Core.Shared.Navigation;
using RaphCare.Mobile.Core.Features.Auth.Views;
using RaphCare.Mobile.Core.Features.Home.Views;

namespace RaphCare.Mobile;

public partial class AppShell : Shell
{
    public AppShell()
    {
        InitializeComponent();
        AppNavigator.RegisterAllRoutes();
    }
}
