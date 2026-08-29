using System.Windows.Input;
using RaphCare.Mobile.Core.Common.Navigation;
using RaphCare.Mobile.Core.Common.Services.FeatureFlags;
using RaphCare.Mobile.Core.Common.ViewModels;

namespace RaphCare.Mobile.Core.Features.Auth.ViewModels;

/// <summary>Create account: Email (password + verification). Phone and Voice options appear when their feature flags are on.</summary>
public class RegisterOptionsViewModel : BaseViewModel
{
    public RegisterOptionsViewModel()
    {
        Title = T("RegisterCreateAccountTitle");
        PageTitle = T("RegisterCreateAccountTitle");
        Subtitle = T("RegisterChooseHowSubtitle");
        EmailTitle = T("RegisterOptionEmailTitle");
        EmailSubtitle = T("RegisterOptionEmailSubtitle");
        PhoneTitle = T("RegisterOptionPhoneTitle");
        PhoneSubtitle = T("RegisterOptionPhoneSubtitle");
        VoiceTitle = T("RegisterOptionVoiceTitle");
        VoiceSubtitle = T("RegisterOptionVoiceSubtitle");
        AlreadyHaveAccount = T("RegisterAlreadyHaveAccount");
        SignInText = T("AuthSignIn");

        IsPhoneRegistrationVisible = FeatureFlags.PhoneRegistrationEnabled;
        IsVoiceRegistrationVisible = FeatureFlags.VoiceRegistrationEnabled;

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

    public bool IsPhoneRegistrationVisible { get; }
    public bool IsVoiceRegistrationVisible { get; }

    public ICommand CreateWithEmailCommand { get; }
    public ICommand CreateWithPhoneCommand { get; }
    public ICommand CreateWithVoiceCommand { get; }
    public ICommand BackCommand { get; }
    public ICommand SignInCommand { get; }

    private static async Task GoBackAsync()
    {
        if (Shell.Current.Navigation.NavigationStack.Count > 1)
            await SafeShellNavigator.GoToAsync("..");
        else
            await SafeShellNavigator.GoToAsync($"//{AppNavigator.Landing}");
    }
}
