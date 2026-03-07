using RaphCare.Mobile.Features.Auth.Views;
using RaphCare.Mobile.Features.Home.Views;
using RaphCare.Mobile.Shared.Navigation;

namespace RaphCare.Mobile;

public partial class AppShell : Shell
{
    public AppShell()
    {
        InitializeComponent();
        AppNavigator.RegisterAllRoutes();
    }
}
