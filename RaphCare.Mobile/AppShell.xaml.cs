using RaphCare.Mobile.Features.Auth.Views;
using RaphCare.Mobile.Features.Home.Views;

namespace RaphCare.Mobile;

public partial class AppShell : Shell
{
    public AppShell()
    {
        InitializeComponent();

        // Register routes for navigation (optional when using ContentTemplate with Route)
        Routing.RegisterRoute(nameof(LandingPage), typeof(LandingPage));
        Routing.RegisterRoute(nameof(RegisterOptionsPage), typeof(RegisterOptionsPage));
        Routing.RegisterRoute(nameof(RegisterEmailPage), typeof(RegisterEmailPage));
        Routing.RegisterRoute(nameof(VerifyEmailPage), typeof(VerifyEmailPage));
        Routing.RegisterRoute(nameof(SignInPage), typeof(SignInPage));
        Routing.RegisterRoute(nameof(HomePage), typeof(HomePage));
    }
}
