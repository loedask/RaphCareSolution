using System.Windows.Input;
using RaphCare.Mobile.Core.Common.Navigation;
using RaphCare.Mobile.Core.Common.ViewModels;
using RaphCare.Mobile.Resources.Strings;

namespace RaphCare.Mobile.Core.Features.Auth.ViewModels;

/// <summary>Voice registration intro: next step is phone OTP (required by API), then voice file upload.</summary>
public class RegisterVoiceIntroViewModel : BaseViewModel
{
    public RegisterVoiceIntroViewModel()
    {
        Title = AppResources.T("RegisterVoiceTitle");
        IntroBody = AppResources.T("RegisterVoiceIntroBody");
        ContinueLabel = AppResources.T("RegisterVoiceContinue");

        ContinueCommand = new Command(async () => await SafeShellNavigator.GoToAsync("RegisterPhonePage?ContinueWith=Voice"));
        BackCommand = new Command(async () => await GoBackAsync());
    }

    public string IntroBody { get; }
    public string ContinueLabel { get; }

    public ICommand ContinueCommand { get; }
    public ICommand BackCommand { get; }

    private static async Task GoBackAsync() =>
        await SafeShellNavigator.GoToAsync("..");
}
