using RaphCare.Mobile.Core.Common.Navigation;
using RaphCare.Mobile.Core.Features.Devices.ViewModels;

namespace RaphCare.Mobile.Core.Features.Devices.Views;

public partial class WatchReadingsPage : ContentPage
{
    public WatchReadingsPage(WatchReadingsViewModel viewModel)
    {
        InitializeComponent();
        BindingContext = viewModel;
    }

    protected override async void OnAppearing()
    {
        base.OnAppearing();
        if (BindingContext is WatchReadingsViewModel vm)
            await SafePageLoad.RunAsync(() => vm.OnAppearingAsync());
    }

    protected override void OnDisappearing()
    {
        base.OnDisappearing();
        if (BindingContext is WatchReadingsViewModel vm)
            vm.Dispose();
    }
}
