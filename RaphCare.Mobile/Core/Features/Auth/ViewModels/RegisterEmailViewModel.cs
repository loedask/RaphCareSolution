using System.Windows.Input;
using RaphCare.Mobile.Core.Shared.Navigation;
using RaphCare.Mobile.Core.Shared.Services.Auth;
using RaphCare.Mobile.Core.Shared.ViewModels;
using RaphCare.Mobile.Resources.Strings;

namespace RaphCare.Mobile.Core.Features.Auth.ViewModels;

/// <summary>
/// Email registration form. Triggers Entra sign-up; on success navigates to VerifyEmailPage.
/// </summary>
public class RegisterEmailViewModel : BaseViewModel
{
    private readonly IAuthService _authService;

    private string _firstName = string.Empty;
    private string _lastName = string.Empty;
    private string _email = string.Empty;
    private string _password = string.Empty;
    private string? _errorMessage;

    public RegisterEmailViewModel(IAuthService authService)
    {
        _authService = authService ?? throw new ArgumentNullException(nameof(authService));
        Title = AppResources.T("RegisterEmailTitle");
        PageTitle = AppResources.T("RegisterEmailTitle");
        Subtitle = AppResources.T("RegisterEmailSubtitle");
        FirstNameLabel = AppResources.T("RegisterEmailFirstName");
        LastNameLabel = AppResources.T("RegisterEmailLastName");
        EmailLabel = AppResources.T("RegisterEmailEmail");
        PasswordLabel = AppResources.T("RegisterEmailPassword");
        PlaceholderFirst = AppResources.T("RegisterEmailPlaceholderFirst");
        PlaceholderLast = AppResources.T("RegisterEmailPlaceholderLast");
        PlaceholderEmail = AppResources.T("RegisterEmailPlaceholderEmail");
        PlaceholderPassword = AppResources.T("RegisterEmailPlaceholderPassword");
        ContinueText = AppResources.T("RegisterEmailContinue");
        AlreadyHaveText = AppResources.T("RegisterEmailAlreadyHave");
        SignInLinkText = AppResources.T("RegisterEmailSignIn");

        RegisterCommand = new Command(async () => await RegisterAsync(), () => !IsBusy);
        BackCommand = new Command(async () => await GoBackAsync());
        SignInCommand = new Command(async () => await SafeShellNavigator.GoToAsync("SignInPage"));
    }

    public string PageTitle { get; }
    public string Subtitle { get; }
    public string FirstNameLabel { get; }
    public string LastNameLabel { get; }
    public string EmailLabel { get; }
    public string PasswordLabel { get; }
    public string PlaceholderFirst { get; }
    public string PlaceholderLast { get; }
    public string PlaceholderEmail { get; }
    public string PlaceholderPassword { get; }
    public string ContinueText { get; }
    public string AlreadyHaveText { get; }
    public string SignInLinkText { get; }

    public string FirstName
    {
        get => _firstName;
        set => SetProperty(ref _firstName, value ?? string.Empty);
    }

    public string LastName
    {
        get => _lastName;
        set => SetProperty(ref _lastName, value ?? string.Empty);
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

    public string? ErrorMessage
    {
        get => _errorMessage;
        set => SetProperty(ref _errorMessage, value);
    }

    public ICommand RegisterCommand { get; }
    public ICommand BackCommand { get; }
    public ICommand SignInCommand { get; }

    private async Task RegisterAsync()
    {
        if (IsBusy) return;

        ErrorMessage = null;
        if (string.IsNullOrWhiteSpace(FirstName))
        {
            ErrorMessage = AppResources.T("RegisterEmailErrorFirstName");
            return;
        }

        if (string.IsNullOrWhiteSpace(LastName))
        {
            ErrorMessage = AppResources.T("RegisterEmailErrorLastName");
            return;
        }

        if (string.IsNullOrWhiteSpace(Email))
        {
            ErrorMessage = AppResources.T("RegisterEmailErrorEmail");
            return;
        }

        if (string.IsNullOrWhiteSpace(Password))
        {
            ErrorMessage = AppResources.T("RegisterEmailErrorPassword");
            return;
        }

        IsBusy = true;
        try
        {
            var result = await _authService
                .RegisterWithEmailAsync(FirstName, LastName, Email.Trim(), Password, CancellationToken.None)
                .ConfigureAwait(false);

            if (result.Success)
            {
                await SafeShellNavigator.GoToAsync("//HomePage").ConfigureAwait(false);
            }
            else
            {
                ErrorMessage = result.ErrorMessage ?? AppResources.T("RegisterEmailFailed");
            }
        }
        catch (Exception)
        {
            ErrorMessage = AppResources.T("RegisterEmailFailed");
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
            await SafeShellNavigator.GoToAsync("RegisterOptionsPage");
    }
}
