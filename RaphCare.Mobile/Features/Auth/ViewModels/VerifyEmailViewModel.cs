using System.Windows.Input;
using RaphCare.Mobile.Core.ViewModels;
using RaphCare.Mobile.Features.Auth.Services;

namespace RaphCare.Mobile.Features.Auth.ViewModels;

/// <summary>
/// Email verification prompt. User verifies via Entra; then Sign In navigates to sign-in and then Home.
/// </summary>
public class VerifyEmailViewModel : BaseViewModel
{
    private readonly IAuthService _authService;

    private string? _errorMessage;

    public string? ErrorMessage
    {
        get => _errorMessage;
        set => SetProperty(ref _errorMessage, value);
    }

    public ICommand SignInCommand { get; }
    public ICommand ResendCommand { get; }

    public VerifyEmailViewModel(IAuthService authService)
    {
        _authService = authService ?? throw new ArgumentNullException(nameof(authService));
        Title = "Verify Email";
        SignInCommand = new Command(async () => await SignInAsync(), () => !IsBusy);
        ResendCommand = new Command(async () => await ResendAsync(), () => !IsBusy);
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

    private async Task ResendAsync()
    {
        if (IsBusy) return;
        IsBusy = true;
        try
        {
            // Entra handles verification; resend is typically done from the verification email link.
            ErrorMessage = null;
            await Task.CompletedTask.ConfigureAwait(false);
        }
        finally
        {
            IsBusy = false;
        }
    }
}
