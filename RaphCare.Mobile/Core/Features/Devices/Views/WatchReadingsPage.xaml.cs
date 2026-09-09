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
        {
            vm.AttachBleHandlers();
            await SafePageLoad.RunAsync(() => vm.OnAppearingAsync());
        }
    }

    protected override void OnDisappearing()
    {
        base.OnDisappearing();
        // Do not Dispose here: Shell can keep the page; hard dispose left Home blank after
        // Measure / vendor callbacks. Detach only; Dispose when the page is finalized.
        if (BindingContext is WatchReadingsViewModel vm)
            vm.DetachBleHandlers();
    }
}
