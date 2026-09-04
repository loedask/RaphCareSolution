using RaphCare.Mobile.Core.Features.Settings.ViewModels;
using RaphCare.Mobile.Core.Infrastructure.Composition;
using RaphCare.Mobile.Core.Common.Navigation;

namespace RaphCare.Mobile.Core.Features.Settings.Views;

public partial class ChangePasswordPage : ContentPage
{
    public ChangePasswordPage() : this(MobileServiceHub.GetRequiredService<ChangePasswordViewModel>()) { }

    public ChangePasswordPage(ChangePasswordViewModel viewModel)
    {
        InitializeComponent();
        BindingContext = viewModel;
    }

    protected override async void OnAppearing()
    {
        base.OnAppearing();
        if (BindingContext is ChangePasswordViewModel vm)
            await SafePageLoad.RunAsync(() => vm.LoadAsync());
    }
}
