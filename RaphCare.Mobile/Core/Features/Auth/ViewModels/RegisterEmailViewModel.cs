using System.Windows.Input;
using RaphCare.Mobile.Core.Shared.Services.Auth;
using RaphCare.Mobile.Core.Shared.ViewModels;

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

    public RegisterEmailViewModel(IAuthService authService)
    {
        _authService = authService ?? throw new ArgumentNullException(nameof(authService));
        Title = "Create with Email";
        RegisterCommand = new Command(async () => await RegisterAsync(), () => !IsBusy);
        BackCommand = new Command(async () => await GoBackAsync());
        SignInCommand = new Command(async () => await Shell.Current.GoToAsync("SignInPage").ConfigureAwait(false));
    }

    private async Task RegisterAsync()
    {
        if (IsBusy) return;

        ErrorMessage = null;
        if (string.IsNullOrWhiteSpace(Email))
        {
            ErrorMessage = "Please enter your email.";
            return;
        }
        if (string.IsNullOrWhiteSpace(Password))
        {
            ErrorMessage = "Please enter a password.";
            return;
        }

        IsBusy = true;
        try
        {
            var result = await _authService.SignUpWithEmailAsync(Email.Trim(), Password, CancellationToken.None).ConfigureAwait(false);

            if (result.Success)
            {
                await Shell.Current.GoToAsync("//VerifyEmailPage").ConfigureAwait(false);
            }
            else
            {
                ErrorMessage = result.ErrorMessage ?? "Registration failed.";
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
            await Shell.Current.GoToAsync("RegisterOptionsPage").ConfigureAwait(false);
    }
}
