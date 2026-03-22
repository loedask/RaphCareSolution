using RaphCare.Mobile.Core.Features.Auth.ViewModels;
using RaphCare.Mobile.Core.Infrastructure.Composition;

namespace RaphCare.Mobile.Core.Features.Auth.Views;

public partial class RegisterEmailPage : ContentPage
{
    public RegisterEmailPage() : this(MobileServiceHub.GetRequiredService<RegisterEmailViewModel>()) { }

    public RegisterEmailPage(RegisterEmailViewModel viewModel)
    {
        InitializeComponent();
        BindingContext = viewModel;
    }
}
