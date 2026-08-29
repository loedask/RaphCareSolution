using System.Collections.ObjectModel;
using System.Windows.Input;
using RaphCare.Client.Contracts.Interfaces;
using RaphCare.Mobile.Core.Features.Auth.Models;
using RaphCare.Mobile.Core.Common.Navigation;
using RaphCare.Mobile.Core.Common.Services.Api;
using RaphCare.Mobile.Core.Common.Services.Auth;
using RaphCare.Mobile.Core.Common.ViewModels;

namespace RaphCare.Mobile.Core.Features.Auth.ViewModels;

/// <summary>
/// Email/password patient registration: details first, then email verification code, with optional clinic selection.
/// </summary>
public class RegisterEmailViewModel : BaseViewModel
{
    private readonly IAuthService _authService;
    private readonly IEmailAuthService _emailAuthService;
    private readonly ISelectedClinicStore _selectedClinicStore;

    private string _firstName = string.Empty;
    private string _lastName = string.Empty;
    private string _email = string.Empty;
    private string _password = string.Empty;
    private string _verificationCode = string.Empty;
    private string _clinicSearchText = string.Empty;
    private ClinicPickerItem? _selectedClinic;
    private string? _errorMessage;
    private string? _statusMessage;
    private bool _sendingCode;
    private bool _awaitingVerification;

    public RegisterEmailViewModel(
        IAuthService authService,
        IEmailAuthService emailAuthService,
        ISelectedClinicStore selectedClinicStore)
    {
        _authService = authService ?? throw new ArgumentNullException(nameof(authService));
        _emailAuthService = emailAuthService ?? throw new ArgumentNullException(nameof(emailAuthService));
        _selectedClinicStore = selectedClinicStore ?? throw new ArgumentNullException(nameof(selectedClinicStore));
        Title = T("RegisterEmailTitle");
        PageTitle = T("RegisterEmailTitle");
        DetailsSubtitle = T("RegisterEmailSubtitle");
        VerifySubtitle = T("RegisterEmailVerifySubtitle");
        FirstNameLabel = T("RegisterEmailFirstName");
        LastNameLabel = T("RegisterEmailLastName");
        EmailLabel = T("RegisterEmailEmail");
        PasswordLabel = T("RegisterEmailPassword");
        VerificationCodeLabel = T("RegisterEmailVerificationCode");
        ClinicLabel = T("RegisterEmailClinic");
        ClinicHint = T("RegisterEmailClinicHint");
        ClinicSearchPlaceholder = T("RegisterEmailClinicSearchPlaceholder");
        ClinicSearchButtonText = T("RegisterEmailClinicSearchButton");
        PlaceholderFirst = T("RegisterEmailPlaceholderFirst");
        PlaceholderLast = T("RegisterEmailPlaceholderLast");
        PlaceholderEmail = T("RegisterEmailPlaceholderEmail");
        PlaceholderPassword = T("RegisterEmailPlaceholderPassword");
        PlaceholderVerificationCode = T("RegisterEmailVerificationPlaceholder");
        ContinueLabel = T("RegisterEmailContinue");
        VerifyButtonLabel = T("RegisterEmailVerifyButton");
        SendCodeText = T("RegisterEmailSendCode");
        AlreadyHaveText = T("RegisterEmailAlreadyHave");
        SignInLinkText = T("RegisterEmailSignIn");

        Clinics = new ObservableCollection<ClinicPickerItem>();

        ContinueCommand = new Command(async () => await ContinueAsync(), () => !IsBusy);
        SendCodeCommand = new Command(async () => await SendCodeAsync(), () => !IsBusy && !SendingCode);
        SearchClinicsCommand = new Command(async () => await LoadClinicsAsync(), () => !IsBusy);
        BackCommand = new Command(async () => await GoBackAsync());
        SignInCommand = new Command(async () => await SafeShellNavigator.GoToAsync("SignInPage"));
    }

    public ObservableCollection<ClinicPickerItem> Clinics { get; }

    public string PageTitle { get; }
    public string DetailsSubtitle { get; }
    public string VerifySubtitle { get; }
    public string FirstNameLabel { get; }
    public string LastNameLabel { get; }
    public string EmailLabel { get; }
    public string PasswordLabel { get; }
    public string VerificationCodeLabel { get; }
    public string ClinicLabel { get; }
    public string ClinicHint { get; }
    public string ClinicSearchPlaceholder { get; }
    public string ClinicSearchButtonText { get; }
    public string PlaceholderFirst { get; }
    public string PlaceholderLast { get; }
    public string PlaceholderEmail { get; }
    public string PlaceholderPassword { get; }
    public string PlaceholderVerificationCode { get; }
    public string ContinueLabel { get; }
    public string VerifyButtonLabel { get; }
    public string SendCodeText { get; }
    public string AlreadyHaveText { get; }
    public string SignInLinkText { get; }

    public string Subtitle => AwaitingVerification ? VerifySubtitle : DetailsSubtitle;

    public string ContinueText => AwaitingVerification ? VerifyButtonLabel : ContinueLabel;

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

    public string VerificationCode
    {
        get => _verificationCode;
        set => SetProperty(ref _verificationCode, value ?? string.Empty);
    }

    public string ClinicSearchText
    {
        get => _clinicSearchText;
        set => SetProperty(ref _clinicSearchText, value ?? string.Empty);
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

    public string? StatusMessage
    {
        get => _statusMessage;
        set => SetProperty(ref _statusMessage, value);
    }

    public bool AwaitingVerification
    {
        get => _awaitingVerification;
        private set
        {
            if (_awaitingVerification == value) return;
            _awaitingVerification = value;
            OnPropertyChanged();
            OnPropertyChanged(nameof(Subtitle));
            OnPropertyChanged(nameof(ContinueText));
        }
    }

    public bool SendingCode
    {
        get => _sendingCode;
        set
        {
            if (_sendingCode == value) return;
            _sendingCode = value;
            OnPropertyChanged();
            OnPropertyChanged(nameof(SendCodeButtonText));
            (SendCodeCommand as Command)?.ChangeCanExecute();
        }
    }

    public string SendCodeButtonText =>
        SendingCode ? T("RegisterEmailSendingCode") : SendCodeText;

    public ICommand ContinueCommand { get; }
    public ICommand SendCodeCommand { get; }
    public ICommand SearchClinicsCommand { get; }
    public ICommand BackCommand { get; }
    public ICommand SignInCommand { get; }

    public async Task LoadClinicsAsync()
    {
        ErrorMessage = null;
        IsBusy = true;
        BusyMessage = T("CommonLoadingShort");
        try
        {
            var search = string.IsNullOrWhiteSpace(ClinicSearchText) ? null : ClinicSearchText.Trim();
            var response = await _emailAuthService
                .GetRegistrationClinicsAsync(search, CancellationToken.None)
                .ConfigureAwait(false);
            Clinics.Clear();
            Clinics.Add(new ClinicPickerItem
            {
                Id = null,
                Name = T("RegisterEmailClinicNone"),
                DisplayName = T("RegisterEmailClinicNone")
            });

            if (response.IsSuccess && response.Data is not null)
            {
                foreach (var clinic in response.Data)
                {
                    var code = clinic.ReferenceCode?.Trim() ?? string.Empty;
                    var display = string.IsNullOrEmpty(code)
                        ? clinic.Name
                        : $"{clinic.Name} ({code})";
                    Clinics.Add(new ClinicPickerItem
                    {
                        Id = clinic.Id,
                        Name = clinic.Name,
                        ReferenceCode = code,
                        DisplayName = display
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
                    Name = T("RegisterEmailClinicNone"),
                    DisplayName = T("RegisterEmailClinicNone")
                });
                SelectedClinic = Clinics[0];
            }
        }
        finally
        {
            IsBusy = false;
            (SearchClinicsCommand as Command)?.ChangeCanExecute();
        }
    }

    private async Task ContinueAsync()
    {
        if (IsBusy) return;

        ErrorMessage = null;
        StatusMessage = null;

        if (!AwaitingVerification)
        {
            await AdvanceToVerificationAsync().ConfigureAwait(false);
            return;
        }

        await RegisterAsync().ConfigureAwait(false);
    }

    private async Task AdvanceToVerificationAsync()
    {
        if (!ValidateDetails())
            return;

        IsBusy = true;
        BusyMessage = T("RegisterEmailSendingBusy");
        try
        {
            var sent = await TrySendCodeAsync().ConfigureAwait(false);
            if (!sent)
                return;

            VerificationCode = string.Empty;
            AwaitingVerification = true;
            StatusMessage = T("RegisterEmailCodeSent");
        }
        finally
        {
            IsBusy = false;
        }
    }

    private async Task SendCodeAsync()
    {
        if (IsBusy || SendingCode) return;

        ErrorMessage = null;
        StatusMessage = null;
        if (string.IsNullOrWhiteSpace(Email))
        {
            ErrorMessage = T("RegisterEmailErrorEmail");
            return;
        }

        SendingCode = true;
        try
        {
            await TrySendCodeAsync().ConfigureAwait(false);
        }
        finally
        {
            SendingCode = false;
        }
    }

    private async Task<bool> TrySendCodeAsync()
    {
        try
        {
            var response = await _emailAuthService
                .SendEmailVerificationAsync(Email.Trim(), CancellationToken.None)
                .ConfigureAwait(false);

            if (response.IsSuccess)
            {
                StatusMessage = T("RegisterEmailCodeSent");
                return true;
            }

            ErrorMessage = response.ErrorMessage ?? T("RegisterEmailSendCodeFailed");
            return false;
        }
        catch (Exception)
        {
            ErrorMessage = T("RegisterEmailSendCodeFailed");
            return false;
        }
    }

    private async Task RegisterAsync()
    {
        if (string.IsNullOrWhiteSpace(VerificationCode))
        {
            ErrorMessage = T("RegisterEmailErrorVerificationCode");
            return;
        }

        IsBusy = true;
        BusyMessage = T("RegisterEmailCreatingBusy");
        try
        {
            var clinicId = SelectedClinic?.Id;
            var result = await _authService
                .RegisterWithEmailAsync(
                    FirstName,
                    LastName,
                    Email.Trim(),
                    Password,
                    clinicId,
                    VerificationCode.Trim(),
                    CancellationToken.None)
                .ConfigureAwait(false);

            if (result.Success)
            {
                if (SelectedClinic?.Id is { } selectedId)
                {
                    _selectedClinicStore.SetClinic(
                        selectedId,
                        SelectedClinic.Name,
                        SelectedClinic.ReferenceCode);
                }

                await SafeShellNavigator.GoToAsync($"//{AppNavigator.AccountCreated}").ConfigureAwait(false);
            }
            else
            {
                ErrorMessage = result.ErrorMessage ?? T("RegisterEmailFailed");
            }
        }
        catch (Exception)
        {
            ErrorMessage = T("RegisterEmailFailed");
        }
        finally
        {
            IsBusy = false;
        }
    }

    private bool ValidateDetails()
    {
        if (string.IsNullOrWhiteSpace(FirstName))
        {
            ErrorMessage = T("RegisterEmailErrorFirstName");
            return false;
        }

        if (string.IsNullOrWhiteSpace(LastName))
        {
            ErrorMessage = T("RegisterEmailErrorLastName");
            return false;
        }

        if (string.IsNullOrWhiteSpace(Email))
        {
            ErrorMessage = T("RegisterEmailErrorEmail");
            return false;
        }

        if (string.IsNullOrWhiteSpace(Password))
        {
            ErrorMessage = T("RegisterEmailErrorPassword");
            return false;
        }

        return true;
    }

    private async Task GoBackAsync()
    {
        if (AwaitingVerification)
        {
            AwaitingVerification = false;
            VerificationCode = string.Empty;
            ErrorMessage = null;
            StatusMessage = null;
            return;
        }

        if (Shell.Current.Navigation.NavigationStack.Count > 1)
            await SafeShellNavigator.GoToAsync("..");
        else
            await SafeShellNavigator.GoToAsync("RegisterOptionsPage");
    }
}
