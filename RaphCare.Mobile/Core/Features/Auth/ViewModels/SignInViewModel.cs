using System.Windows.Input;
using RaphCare.Mobile.Core.Shared.Services.Auth;
using RaphCare.Mobile.Core.Shared.ViewModels;

namespace RaphCare.Mobile.Core.Features.Auth.ViewModels;

/// <summary>
/// Sign-in with Entra ID. On success navigates to HomePage.
/// </summary>
public class SignInViewModel : BaseViewModel
{
    private readonly IAuthService _authService;

    private string? _errorMessage;

    public string? ErrorMessage
    {
        get => _errorMessage;
        set => SetProperty(ref _errorMessage, value);
    }

    public ICommand SignInCommand { get; }
    public ICommand BackCommand { get; }
    public ICommand SignUpCommand { get; }

    public SignInViewModel(IAuthService authService)
    {
        _authService = authService ?? throw new ArgumentNullException(nameof(authService));
        Title = "Sign In";
        SignInCommand = new Command(async () => await SignInAsync(), () => !IsBusy);
        BackCommand = new Command(async () => await GoBackAsync());
        SignUpCommand = new Command(async () => await Shell.Current.GoToAsync("RegisterOptionsPage").ConfigureAwait(false));
    }

    private async Task SignInAsync()
    {
        if (IsBusy) return;

        ErrorMessage = null;
        IsBusy = true;
        try
        {
            var result = await _authService.SignInAsync(CancellationToken.None).ConfigureAwait(false);

            if (result.Success)
            {
                await Shell.Current.GoToAsync("//HomePage").ConfigureAwait(false);
            }
            else
            {
                ErrorMessage = result.ErrorMessage ?? "Sign-in failed.";
            }
        }
        finally
        {
            IsBusy = false;
        }
    }

    private async Task GoBackAsync()
    {
        if (Shell.Current.Navigation.NavigationStack.Count > 1)
            await Shell.Current.GoToAsync("..").ConfigureAwait(false);
        else
            await Shell.Current.GoToAsync("//LandingPage").ConfigureAwait(false);
    }
}
