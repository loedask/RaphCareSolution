using RaphCare.Mobile.Core.Common.Navigation;
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

    protected override async void OnAppearing()
    {
        base.OnAppearing();
        if (BindingContext is RegisterEmailViewModel vm)
            await SafePageLoad.RunAsync(() => vm.LoadClinicsAsync());
    }
}
