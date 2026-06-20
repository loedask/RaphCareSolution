using System.Windows.Input;
using Microsoft.Extensions.Options;
using Microsoft.Maui.ApplicationModel;
using RaphCare.Mobile.Core.Shared.Navigation;
using RaphCare.Mobile.Resources.Strings;
using RaphCare.Mobile.Core.Shared.Services.Auth;
using RaphCare.Mobile.Core.Shared.ViewModels;

namespace RaphCare.Mobile.Core.Features.Auth.ViewModels;

/// <summary>
/// Sign-in with email and password. After password validation, a verification code is emailed for two-factor sign-in.
/// </summary>
public class SignInViewModel : BaseViewModel
{
    private readonly IAuthService _authService;
    private readonly EntraAuthOptions _options;

    private string? _errorMessage;
    private string? _statusMessage;
    private string _email = string.Empty;
    private string _password = string.Empty;
    private string _verificationCode = string.Empty;
    private bool _awaitingVerification;

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

    public string WelcomeBack => AppResources.T("AuthWelcomeBack");

    public string SignInSubtitle => AppResources.T("AuthSignInToContinue");

    public string EmailLabel => AppResources.T("AuthEmailLabel");
    public string PasswordLabel => AppResources.T("AuthPasswordLabel");
    public string VerificationCodeLabel => AppResources.T("AuthVerificationCodeLabel");
    public string EmailPlaceholder => AppResources.T("AuthEmailPlaceholder");
    public string PasswordPlaceholder => AppResources.T("AuthPasswordPlaceholder");
    public string VerificationCodePlaceholder => AppResources.T("AuthVerificationCodePlaceholder");

    public string SignInButtonText =>
        AwaitingVerification ? AppResources.T("AuthSignInVerifyButton") : AppResources.T("AuthSignIn");

    public string ForgotPasswordText => AppResources.T("AuthForgotPassword");

    public string DontHaveAccountText => AppResources.T("AuthDontHaveAccount");

    public string SignUpText => AppResources.T("AuthSignUp");

    public bool AwaitingVerification
    {
        get => _awaitingVerification;
        private set
        {
            if (_awaitingVerification == value) return;
            _awaitingVerification = value;
            OnPropertyChanged();
            OnPropertyChanged(nameof(SignInButtonText));
        }
    }

    public string Email
    {
        get => _email;
        set => SetProperty(ref _email, value ?? string.Empty);
    }

    public string Password
    {
        get => _password;
        set => SetProperty(ref _password, value ?? string.Empty);
    }

    public string VerificationCode
    {
        get => _verificationCode;
        set => SetProperty(ref _verificationCode, value ?? string.Empty);
    }

    public ICommand SignInCommand { get; }
    public ICommand BackCommand { get; }
    public ICommand SignUpCommand { get; }
    public ICommand ForgotPasswordCommand { get; }

    public SignInViewModel(IAuthService authService, IOptions<EntraAuthOptions> options)
    {
        _authService = authService ?? throw new ArgumentNullException(nameof(authService));
        _options = options?.Value ?? throw new ArgumentNullException(nameof(options));
        Title = AppResources.T("AuthSignInPageTitle");
        SignInCommand = new Command(async () => await SignInAsync().ConfigureAwait(false), () => !IsBusy);
        BackCommand = new Command(async () => await GoBackAsync().ConfigureAwait(false));
        SignUpCommand = new Command(async () => await OpenSignUpAsync().ConfigureAwait(false));
        ForgotPasswordCommand = new Command(async () => await OpenPasswordResetAsync().ConfigureAwait(false));
    }

    private async Task OpenSignUpAsync()
    {
        await SafeShellNavigator.GoToAsync("RegisterOptionsPage").ConfigureAwait(false);
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
        StatusMessage = null;
        if (string.IsNullOrWhiteSpace(Email))
        {
            ErrorMessage = AppResources.T("AuthEmailRequired");
            return;
        }

        if (string.IsNullOrWhiteSpace(Password))
        {
            ErrorMessage = AppResources.T("AuthPasswordRequired");
            return;
        }

        if (AwaitingVerification && string.IsNullOrWhiteSpace(VerificationCode))
        {
            ErrorMessage = AppResources.T("RegisterEmailErrorVerificationCode");
            return;
        }

        IsBusy = true;
        try
        {
            var code = AwaitingVerification ? VerificationCode.Trim() : null;
            var result = await _authService
                .SignInWithEmailAsync(Email.Trim(), Password, code, CancellationToken.None)
                .ConfigureAwait(false);

            if (result.RequiresVerification)
            {
                AwaitingVerification = true;
                StatusMessage = AppResources.T("AuthSignInCodeSent");
                return;
            }

            if (result.Success)
            {
                await SafeShellNavigator.GoToAsync("//HomePage").ConfigureAwait(false);
            }
            else
            {
                ErrorMessage = result.ErrorMessage ?? AppResources.T("AuthSignInFailed");
            }
        }
        catch (Exception)
        {
            ErrorMessage = AppResources.T("AuthSignInFailed");
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
