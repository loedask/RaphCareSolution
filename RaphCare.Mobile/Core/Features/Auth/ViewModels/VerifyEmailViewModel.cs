using System.Windows.Input;
using RaphCare.Mobile.Core.Shared.Navigation;
using RaphCare.Mobile.Core.Shared.Services.Auth;
using RaphCare.Mobile.Core.Shared.ViewModels;
using RaphCare.Mobile.Resources.Strings;

namespace RaphCare.Mobile.Core.Features.Auth.ViewModels;

/// <summary>
/// Email verification prompt. User verifies via Entra; Sign In on success goes to <c>AccountCreatedPage</c>, then home.
/// </summary>
public class VerifyEmailViewModel : BaseViewModel
{
    private readonly IAuthService _authService;

    private string? _errorMessage;
    private string? _statusMessage;

    public VerifyEmailViewModel(IAuthService authService)
    {
        _authService = authService ?? throw new ArgumentNullException(nameof(authService));
        Title = AppResources.T("VerifyEmailPageTitle");
        Headline = AppResources.T("VerifyEmailTitle");
        Subtitle = AppResources.T("VerifyEmailSubtitle");
        SignInButtonText = AppResources.T("VerifyEmailSignIn");
        ResendButtonText = AppResources.T("VerifyEmailResend");

        SignInCommand = new Command(async () => await SignInAsync(), () => !IsBusy);
        ResendCommand = new Command(async () => await ResendAsync(), () => !IsBusy);
    }

    public string Headline { get; }
    public string Subtitle { get; }
    public string SignInButtonText { get; }
    public string ResendButtonText { get; }

    public string? ErrorMessage
    {
        get => _errorMessage;
        set => SetProperty(ref _errorMessage, value);
    }

    public string? StatusMessage
    {
        get => _statusMessage;
        set => SetProperty(ref _statusMessage, value);
    }

    public ICommand SignInCommand { get; }
    public ICommand ResendCommand { get; }

    private async Task SignInAsync()
    {
        if (IsBusy) return;

        ErrorMessage = null;
        StatusMessage = null;
        IsBusy = true;
        try
        {
            var result = await _authService.SignInAsync(CancellationToken.None).ConfigureAwait(false);

            if (result.Success)
            {
                await SafeShellNavigator.GoToAsync($"//{AppNavigator.AccountCreated}").ConfigureAwait(false);
            }
            else
            {
                ErrorMessage = result.ErrorMessage ?? AppResources.T("AuthSignInFailed");
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
        ErrorMessage = null;
        IsBusy = true;
        try
        {
            StatusMessage = AppResources.T("VerifyEmailResendHint");
            await Task.CompletedTask.ConfigureAwait(false);
        }
        finally
        {
            IsBusy = false;
        }
    }
}
