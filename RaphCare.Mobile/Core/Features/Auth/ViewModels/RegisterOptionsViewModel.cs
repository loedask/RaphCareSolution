using System.Windows.Input;
using RaphCare.Mobile.Core.Shared.Navigation;
using RaphCare.Mobile.Core.Shared.ViewModels;
using RaphCare.Mobile.Resources.Strings;

namespace RaphCare.Mobile.Core.Features.Auth.ViewModels;

/// <summary>Create account: Email (Entra), Phone (OTP + API JWT), or Voice (phone OTP then multipart upload).</summary>
public class RegisterOptionsViewModel : BaseViewModel
{
    public RegisterOptionsViewModel()
    {
        Title = AppResources.T("RegisterCreateAccountTitle");
        PageTitle = AppResources.T("RegisterCreateAccountTitle");
        Subtitle = AppResources.T("RegisterChooseHowSubtitle");
        EmailTitle = AppResources.T("RegisterOptionEmailTitle");
        EmailSubtitle = AppResources.T("RegisterOptionEmailSubtitle");
        PhoneTitle = AppResources.T("RegisterOptionPhoneTitle");
        PhoneSubtitle = AppResources.T("RegisterOptionPhoneSubtitle");
        VoiceTitle = AppResources.T("RegisterOptionVoiceTitle");
        VoiceSubtitle = AppResources.T("RegisterOptionVoiceSubtitle");
        AlreadyHaveAccount = AppResources.T("RegisterAlreadyHaveAccount");
        SignInText = AppResources.T("AuthSignIn");

        CreateWithEmailCommand = new Command(async () => await SafeShellNavigator.GoToAsync("RegisterEmailPage"));
        CreateWithPhoneCommand = new Command(async () => await SafeShellNavigator.GoToAsync("RegisterPhonePage"));
        CreateWithVoiceCommand = new Command(async () => await SafeShellNavigator.GoToAsync("RegisterVoiceIntroPage"));
        BackCommand = new Command(async () => await GoBackAsync());
        SignInCommand = new Command(async () => await SafeShellNavigator.GoToAsync("SignInPage"));
    }

    public string PageTitle { get; }
    public string Subtitle { get; }
    public string EmailTitle { get; }
    public string EmailSubtitle { get; }
    public string PhoneTitle { get; }
    public string PhoneSubtitle { get; }
    public string VoiceTitle { get; }
    public string VoiceSubtitle { get; }
    public string AlreadyHaveAccount { get; }
    public string SignInText { get; }

    public ICommand CreateWithEmailCommand { get; }
    public ICommand CreateWithPhoneCommand { get; }
    public ICommand CreateWithVoiceCommand { get; }
    public ICommand BackCommand { get; }
    public ICommand SignInCommand { get; }

    private async Task GoBackAsync()
    {
        if (Shell.Current.Navigation.NavigationStack.Count > 1)
            await SafeShellNavigator.GoToAsync("..");
        else
            await SafeShellNavigator.GoToAsync($"//{AppNavigator.Landing}");
    }
}
