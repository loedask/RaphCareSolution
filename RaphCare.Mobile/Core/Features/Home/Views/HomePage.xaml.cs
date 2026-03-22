using RaphCare.Mobile.Core.Shared.Navigation;
using RaphCare.Mobile.Resources.Strings;

namespace RaphCare.Mobile.Core.Features.Home.Views;

public partial class HomePage : ContentPage
{
    public HomePage()
    {
        InitializeComponent();
        WelcomeLabel.Text = AppResources.HomeWelcome;
        SignedInLabel.Text = AppResources.HomeSignedIn;
        OpenBlazorButton.Text = AppResources.OpenBlazorSample;
    }

    private async void OnOpenBlazorClicked(object? sender, EventArgs e)
    {
        await SafeShellNavigator.GoToAsync(AppNavigator.BlazorHost);
    }
}
