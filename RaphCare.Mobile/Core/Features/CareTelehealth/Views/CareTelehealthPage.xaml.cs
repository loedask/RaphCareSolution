using RaphCare.Mobile.Core.Features.CareTelehealth.ViewModels;
using RaphCare.Mobile.Core.Infrastructure.Composition;

namespace RaphCare.Mobile.Core.Features.CareTelehealth.Views;

public partial class CareTelehealthPage : ContentPage
{
    public CareTelehealthPage() : this(MobileServiceHub.GetRequiredService<CareTelehealthViewModel>()) { }

    public CareTelehealthPage(CareTelehealthViewModel viewModel)
    {
        InitializeComponent();
        BindingContext = viewModel;
    }

    protected override async void OnAppearing()
    {
        base.OnAppearing();
        if (BindingContext is CareTelehealthViewModel vm)
            await vm.LoadAsync();
    }
}
