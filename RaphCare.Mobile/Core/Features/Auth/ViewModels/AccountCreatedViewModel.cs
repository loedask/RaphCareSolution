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

    Headline = T("AccountCreatedTitle");

    Body = T("AccountCreatedSubtitle");

    ContinueLabel = T("AccountCreatedContinue");
        ContinueCommand = new Command(async () => await SafeShellNavigator.GoToAsync("//HomePage"));
    }

    public string Headline { get; }

    public string Body { get; }

    public string ContinueLabel { get; }

    public ICommand ContinueCommand { get; }
}
