using System.Windows.Input;
using RaphCare.Mobile.Core.Common.Navigation;
using RaphCare.Mobile.Core.Common.ViewModels;

namespace RaphCare.Mobile.Core.Features.Auth.ViewModels;

/// <summary>Post-registration celebration (concept: /account-created) before entering the main app.</summary>
public class AccountCreatedViewModel : BaseViewModel
{
    public AccountCreatedViewModel()
    {
        Title = T("AccountCreatedPageTitle");
        ContinueCommand = new Command(async () => await SafeShellNavigator.GoToAsync("//HomePage"));
    }

    public string Headline => T("AccountCreatedTitle");

    public string Body => T("AccountCreatedSubtitle");

    public string ContinueLabel => T("AccountCreatedContinue");

    public ICommand ContinueCommand { get; }
}
