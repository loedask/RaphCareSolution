using RaphCare.Mobile.Core.Features.Settings.ViewModels;
using RaphCare.Mobile.Core.Infrastructure.Composition;
using RaphCare.Mobile.Core.Common.Navigation;

namespace RaphCare.Mobile.Core.Features.Settings.Views;

public partial class SettingsPage : ContentPage
{
    public SettingsPage() : this(MobileServiceHub.GetRequiredService<ProfileHubViewModel>()) { }

    public SettingsPage(ProfileHubViewModel viewModel)
    {
        InitializeComponent();
        BindingContext = viewModel;
    }

    protected override async void OnAppearing()
    {
        base.OnAppearing();
        if (BindingContext is ProfileHubViewModel vm)
            await SafePageLoad.RunAsync(() => vm.LoadAsync());
    }
}
