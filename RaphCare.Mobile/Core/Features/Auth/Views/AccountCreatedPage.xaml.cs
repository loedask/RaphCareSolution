using RaphCare.Mobile.Core.Features.Auth.ViewModels;
using RaphCare.Mobile.Core.Infrastructure.Composition;

namespace RaphCare.Mobile.Core.Features.Auth.Views;

public partial class AccountCreatedPage : ContentPage
{
    public AccountCreatedPage() : this(MobileServiceHub.GetRequiredService<AccountCreatedViewModel>()) { }

    public AccountCreatedPage(AccountCreatedViewModel viewModel)
    {
        InitializeComponent();
        BindingContext = viewModel;
    }
}
