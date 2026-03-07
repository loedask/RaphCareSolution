using System.Windows.Input;
using RaphCare.Mobile.Core.ViewModels;

namespace RaphCare.Mobile.Features.Auth.ViewModels;

/// <summary>
/// Create Account options: choose Email or other providers. Navigates to RegisterEmailPage for email.
/// </summary>
public class RegisterOptionsViewModel : BaseViewModel
{
    public ICommand CreateWithEmailCommand { get; }
    public ICommand BackCommand { get; }

    public RegisterOptionsViewModel()
    {
        Title = "Create Account";
        CreateWithEmailCommand = new Command(async () => await GoToRegisterEmailAsync());
        BackCommand = new Command(async () => await GoBackAsync());
    }

    private async Task GoToRegisterEmailAsync()
    {
        await Shell.Current.GoToAsync("RegisterEmailPage").ConfigureAwait(false);
    }

    private async Task GoBackAsync()
    {
        if (Shell.Current.Navigation.NavigationStack.Count > 1)
            await Shell.Current.GoToAsync("..").ConfigureAwait(false);
        else
            await Shell.Current.GoToAsync("LandingPage").ConfigureAwait(false);
    }
}
