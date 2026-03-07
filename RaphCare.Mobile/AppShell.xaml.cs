using RaphCare.Mobile.Core.Shared.Navigation;
using RaphCare.Mobile.Features.Auth.Views;
using RaphCare.Mobile.Features.Home.Views;

namespace RaphCare.Mobile;

public partial class AppShell : Shell
{
    public AppShell()
    {
        InitializeComponent();
        AppNavigator.RegisterAllRoutes();
    }
}
