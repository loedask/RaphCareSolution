using RaphCare.Mobile.Core.Features.Auth.ViewModels;
using RaphCare.Mobile.Core.Infrastructure.Composition;

namespace RaphCare.Mobile.Core.Features.Auth.Views;

public partial class RegisterPhonePage : ContentPage
{
    public RegisterPhonePage() : this(MobileServiceHub.GetRequiredService<RegisterPhoneViewModel>()) { }

    public RegisterPhonePage(RegisterPhoneViewModel viewModel)
    {
        InitializeComponent();
        BindingContext = viewModel;
    }
}
