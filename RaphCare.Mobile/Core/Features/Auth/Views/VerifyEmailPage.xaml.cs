using RaphCare.Mobile.Core.Features.Auth.ViewModels;
using RaphCare.Mobile.Core.Infrastructure.Composition;

namespace RaphCare.Mobile.Core.Features.Auth.Views;

public partial class VerifyEmailPage : ContentPage
{
    public VerifyEmailPage() : this(MobileServiceHub.GetRequiredService<VerifyEmailViewModel>()) { }

    public VerifyEmailPage(VerifyEmailViewModel viewModel)
    {
        InitializeComponent();
        BindingContext = viewModel;
    }
}
