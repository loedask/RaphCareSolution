using RaphCare.Mobile.Core.Common.Navigation;
using RaphCare.Mobile.Core.Features.Home.ViewModels;
using RaphCare.Mobile.Core.Infrastructure.Composition;

namespace RaphCare.Mobile.Core.Features.Home.Views;

public partial class HomePage : ContentPage
{
    private readonly HomeViewModel _viewModel;

    public HomePage() : this(MobileServiceHub.GetRequiredService<HomeViewModel>()) { }

    public HomePage(HomeViewModel viewModel)
    {
        _viewModel = viewModel;
        InitializeComponent();
        BindingContext = viewModel;
    }

    protected override async void OnAppearing()
    {
        base.OnAppearing();
        // Shell / native BLE callbacks have left Content blank with a null BindingContext.
        if (!ReferenceEquals(BindingContext, _viewModel))
            BindingContext = _viewModel;

        await SafePageLoad.RunAsync(() => _viewModel.RefreshAsync());
    }
}
