using System.Windows.Input;
using RaphCare.Mobile.Core.Shared.Navigation;
using RaphCare.Mobile.Core.Shared.ViewModels;
using RaphCare.Mobile.Resources.Strings;

namespace RaphCare.Mobile.Core.Features.Auth.ViewModels;

/// <summary>Post-registration celebration (concept: /account-created) before entering the main app.</summary>
public class AccountCreatedViewModel : BaseViewModel
{
    public AccountCreatedViewModel()
    {
        Title = AppResources.T("AccountCreatedPageTitle");
        ContinueCommand = new Command(async () => await SafeShellNavigator.GoToAsync("//HomePage"));
    }

    public string Headline => AppResources.T("AccountCreatedTitle");

    public string Body => AppResources.T("AccountCreatedSubtitle");

    public string ContinueLabel => AppResources.T("AccountCreatedContinue");

    public ICommand ContinueCommand { get; }
}
