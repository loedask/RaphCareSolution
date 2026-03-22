using System.Windows.Input;
using Microsoft.Extensions.Options;
using Microsoft.Maui.ApplicationModel;
using RaphCare.Mobile.Core.Shared.Navigation;
using RaphCare.Mobile.Resources.Strings;
using RaphCare.Mobile.Core.Shared.Services.Auth;
using RaphCare.Mobile.Core.Shared.ViewModels;

namespace RaphCare.Mobile.Core.Features.Auth.ViewModels;

/// <summary>
/// Sign-in with Microsoft Entra ID (MSAL interactive). UI matches the concept login layout; credentials are handled in the browser, not in-app fields.
/// </summary>
public class SignInViewModel : BaseViewModel
{
    private readonly IAuthService _authService;
    private readonly EntraAuthOptions _options;

    private string? _errorMessage;

    public string? ErrorMessage
    {
        get => _errorMessage;
        set => SetProperty(ref _errorMessage, value);
    }

    public string WelcomeBack => AppResources.T("AuthWelcomeBack");

    public string SignInSubtitle => AppResources.T("AuthSignInToContinue");

    public string SecureSignInHint => AppResources.T("AuthSignInSecureHint");

    public string SignInButtonText => AppResources.T("AuthSignIn");

    public string ForgotPasswordText => AppResources.T("AuthForgotPassword");

    public string DontHaveAccountText => AppResources.T("AuthDontHaveAccount");

    public string SignUpText => AppResources.T("AuthSignUp");

    public ICommand SignInCommand { get; }
    public ICommand BackCommand { get; }
    public ICommand SignUpCommand { get; }
    public ICommand ForgotPasswordCommand { get; }

    public SignInViewModel(IAuthService authService, IOptions<EntraAuthOptions> options)
    {
        _authService = authService ?? throw new ArgumentNullException(nameof(authService));
        _options = options?.Value ?? throw new ArgumentNullException(nameof(options));
        Title = "Sign In";
        SignInCommand = new Command(async () => await SignInAsync().ConfigureAwait(false), () => !IsBusy);
        BackCommand = new Command(async () => await GoBackAsync().ConfigureAwait(false));
        SignUpCommand = new Command(async () => await OpenSignUpAsync().ConfigureAwait(false));
        ForgotPasswordCommand = new Command(async () => await OpenPasswordResetAsync().ConfigureAwait(false));
    }

    private async Task OpenSignUpAsync()
    {
        var url = _options.ExternalSignUpUrl;
        if (!string.IsNullOrWhiteSpace(url))
            await Launcher.Default.OpenAsync(new Uri(url.Trim(), UriKind.Absolute)).ConfigureAwait(false);
        else
            await SafeShellNavigator.GoToAsync("RegisterOptionsPage");
    }

    private async Task OpenPasswordResetAsync()
    {
        if (!string.IsNullOrWhiteSpace(_options.B2CPasswordResetAuthority))
        {
            ErrorMessage = null;
            IsBusy = true;
            try
            {
                var scopes = _options.GetPasswordResetScopes();
                var result = await _authService
                    .AcquireTokenInteractiveAsync(_options.B2CPasswordResetAuthority.Trim(), scopes, CancellationToken.None)
                    .ConfigureAwait(false);
                if (result.Success)
                    await SafeShellNavigator.GoToAsync("//HomePage").ConfigureAwait(false);
                else
                    ErrorMessage = result.ErrorMessage;
            }
            finally
            {
                IsBusy = false;
            }

            return;
        }

        var url = string.IsNullOrWhiteSpace(_options.SelfServicePasswordResetUrl)
            ? "https://passwordreset.microsoftonline.com/"
            : _options.SelfServicePasswordResetUrl.Trim();
        await Launcher.Default.OpenAsync(new Uri(url, UriKind.Absolute)).ConfigureAwait(false);
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
                await SafeShellNavigator.GoToAsync("//HomePage").ConfigureAwait(false);
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
            await SafeShellNavigator.GoToAsync("..");
        else
            await SafeShellNavigator.GoToAsync("//LandingPage");
    }
}
