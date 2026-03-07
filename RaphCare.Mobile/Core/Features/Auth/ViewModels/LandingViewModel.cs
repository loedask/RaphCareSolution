using System.Windows.Input;
using RaphCare.Mobile.Core.ViewModels;

namespace RaphCare.Mobile.Core.Features.Auth.ViewModels;

/// <summary>
/// Landing (welcome) screen. Navigates to Create Account or Sign In.
/// </summary>
public class LandingViewModel : BaseViewModel
{
    public ICommand CreateAccountCommand { get; }
    public ICommand SignInCommand { get; }

    public LandingViewModel()
    {
        Title = "Welcome";
        CreateAccountCommand = new Command(async () => await GoToRegisterOptionsAsync());
        SignInCommand = new Command(async () => await SignInAsync());
    }

    private async Task GoToRegisterOptionsAsync()
    {
        await Shell.Current.GoToAsync("RegisterOptionsPage").ConfigureAwait(false);
    }

    private async Task SignInAsync()
    {
        await Shell.Current.GoToAsync("SignInPage").ConfigureAwait(false);
    }
}
