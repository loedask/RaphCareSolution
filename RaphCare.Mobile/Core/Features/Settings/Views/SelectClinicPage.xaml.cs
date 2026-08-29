using RaphCare.Mobile.Core.Features.Settings.ViewModels;
using RaphCare.Mobile.Core.Infrastructure.Composition;

namespace RaphCare.Mobile.Core.Features.Settings.Views;

public partial class SelectClinicPage : ContentPage
{
    public SelectClinicPage() : this(MobileServiceHub.GetRequiredService<SelectClinicViewModel>()) { }

    public SelectClinicPage(SelectClinicViewModel viewModel)
    {
        InitializeComponent();
        BindingContext = viewModel;
    }

    protected override async void OnAppearing()
    {
        base.OnAppearing();
        if (BindingContext is SelectClinicViewModel vm)
            await vm.LoadAsync();
    }
}
