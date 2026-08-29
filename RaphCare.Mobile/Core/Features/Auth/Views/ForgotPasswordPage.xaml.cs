using RaphCare.Mobile.Core.Features.Auth.ViewModels;
using RaphCare.Mobile.Core.Infrastructure.Composition;

namespace RaphCare.Mobile.Core.Features.Auth.Views;

[QueryProperty(nameof(EmailQuery), "email")]
public partial class ForgotPasswordPage : ContentPage
{
    public ForgotPasswordPage() : this(MobileServiceHub.GetRequiredService<ForgotPasswordViewModel>()) { }

    public ForgotPasswordPage(ForgotPasswordViewModel viewModel)
    {
        InitializeComponent();
        BindingContext = viewModel;
    }

    public string EmailQuery
    {
        set
        {
            if (BindingContext is ForgotPasswordViewModel vm)
                vm.PrefillEmail(value);
        }
    }
}
