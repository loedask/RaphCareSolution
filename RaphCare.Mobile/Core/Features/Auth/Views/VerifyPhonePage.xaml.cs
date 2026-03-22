using RaphCare.Mobile.Core.Features.Auth.ViewModels;
using RaphCare.Mobile.Core.Infrastructure.Composition;

namespace RaphCare.Mobile.Core.Features.Auth.Views;

public partial class VerifyPhonePage : ContentPage
{
    public VerifyPhonePage() : this(MobileServiceHub.GetRequiredService<VerifyPhoneViewModel>()) { }

    public VerifyPhonePage(VerifyPhoneViewModel viewModel)
    {
        InitializeComponent();
        BindingContext = viewModel;
    }
}
