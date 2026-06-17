using System.Collections.ObjectModel;
using System.Windows.Input;
using RaphCare.Client.Contracts.Interfaces;
using RaphCare.Mobile.Core.Features.Auth.Models;
using RaphCare.Mobile.Core.Shared.Navigation;
using RaphCare.Mobile.Core.Shared.Services.Auth;
using RaphCare.Mobile.Core.Shared.ViewModels;
using RaphCare.Mobile.Resources.Strings;

namespace RaphCare.Mobile.Core.Features.Auth.ViewModels;

/// <summary>Email/password patient registration with optional clinic selection.</summary>
public class RegisterEmailViewModel : BaseViewModel
{
    private readonly IAuthService _authService;
    private readonly IEmailAuthService _emailAuthService;

    private string _firstName = string.Empty;
    private string _lastName = string.Empty;
    private string _email = string.Empty;
    private string _password = string.Empty;
    private ClinicPickerItem? _selectedClinic;
    private string? _errorMessage;

    public RegisterEmailViewModel(IAuthService authService, IEmailAuthService emailAuthService)
    {
        _authService = authService ?? throw new ArgumentNullException(nameof(authService));
        _emailAuthService = emailAuthService ?? throw new ArgumentNullException(nameof(emailAuthService));
        Title = AppResources.T("RegisterEmailTitle");
        PageTitle = AppResources.T("RegisterEmailTitle");
        Subtitle = AppResources.T("RegisterEmailSubtitle");
        FirstNameLabel = AppResources.T("RegisterEmailFirstName");
        LastNameLabel = AppResources.T("RegisterEmailLastName");
        EmailLabel = AppResources.T("RegisterEmailEmail");
        PasswordLabel = AppResources.T("RegisterEmailPassword");
        ClinicLabel = AppResources.T("RegisterEmailClinic");
        ClinicHint = AppResources.T("RegisterEmailClinicHint");
        PlaceholderFirst = AppResources.T("RegisterEmailPlaceholderFirst");
        PlaceholderLast = AppResources.T("RegisterEmailPlaceholderLast");
        PlaceholderEmail = AppResources.T("RegisterEmailPlaceholderEmail");
        PlaceholderPassword = AppResources.T("RegisterEmailPlaceholderPassword");
        ContinueText = AppResources.T("RegisterEmailContinue");
        AlreadyHaveText = AppResources.T("RegisterEmailAlreadyHave");
        SignInLinkText = AppResources.T("RegisterEmailSignIn");

        Clinics = new ObservableCollection<ClinicPickerItem>();

        RegisterCommand = new Command(async () => await RegisterAsync(), () => !IsBusy);
        BackCommand = new Command(async () => await GoBackAsync());
        SignInCommand = new Command(async () => await SafeShellNavigator.GoToAsync("SignInPage"));
    }

    public ObservableCollection<ClinicPickerItem> Clinics { get; }

    public string PageTitle { get; }
    public string Subtitle { get; }
    public string FirstNameLabel { get; }
    public string LastNameLabel { get; }
    public string EmailLabel { get; }
    public string PasswordLabel { get; }
    public string ClinicLabel { get; }
    public string ClinicHint { get; }
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

    public ClinicPickerItem? SelectedClinic
    {
        get => _selectedClinic;
        set => SetProperty(ref _selectedClinic, value);
    }

    public string? ErrorMessage
    {
        get => _errorMessage;
        set => SetProperty(ref _errorMessage, value);
    }

    public ICommand RegisterCommand { get; }
    public ICommand BackCommand { get; }
    public ICommand SignInCommand { get; }

    public async Task LoadClinicsAsync()
    {
        ErrorMessage = null;
        IsBusy = true;
        try
        {
            var response = await _emailAuthService.GetRegistrationClinicsAsync(CancellationToken.None).ConfigureAwait(false);
            Clinics.Clear();
            Clinics.Add(new ClinicPickerItem
            {
                Id = null,
                DisplayName = AppResources.T("RegisterEmailClinicNone")
            });

            if (response.IsSuccess && response.Data is not null)
            {
                foreach (var clinic in response.Data)
                {
                    Clinics.Add(new ClinicPickerItem
                    {
                        Id = clinic.Id,
                        DisplayName = clinic.Name
                    });
                }
            }

            SelectedClinic = Clinics.Count > 0 ? Clinics[0] : null;
        }
        catch (Exception)
        {
            if (Clinics.Count == 0)
            {
                Clinics.Add(new ClinicPickerItem
                {
                    Id = null,
                    DisplayName = AppResources.T("RegisterEmailClinicNone")
                });
                SelectedClinic = Clinics[0];
            }
        }
        finally
        {
            IsBusy = false;
        }
    }

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
            var clinicId = SelectedClinic?.Id;
            var result = await _authService
                .RegisterWithEmailAsync(FirstName, LastName, Email.Trim(), Password, clinicId, CancellationToken.None)
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
