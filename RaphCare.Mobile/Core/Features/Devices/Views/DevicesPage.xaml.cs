using RaphCare.Mobile.Core.Common.Navigation;
using RaphCare.Mobile.Core.Features.Devices.ViewModels;

namespace RaphCare.Mobile.Core.Features.Devices.Views;

public partial class DevicesPage : ContentPage
{
    public DevicesPage(DevicesViewModel viewModel)
    {
        InitializeComponent();
        BindingContext = viewModel;
    }

    protected override async void OnAppearing()
    {
        base.OnAppearing();
        if (BindingContext is DevicesViewModel vm)
            await SafePageLoad.RunAsync(() => vm.OnAppearingAsync());
    }

    protected override void OnDisappearing()
    {
        base.OnDisappearing();
        if (BindingContext is DevicesViewModel vm)
            _ = CleanupAsync(vm);
    }

    private static async Task CleanupAsync(DevicesViewModel vm)
    {
        await vm.OnDisappearingAsync().ConfigureAwait(false);
        vm.DetachBleHandlers();
    }
}
