using RaphCare.Mobile.Core.Features.Settings.ViewModels;
using RaphCare.Mobile.Core.Infrastructure.Composition;

namespace RaphCare.Mobile.Core.Features.Settings.Views;

public partial class EmergencyContactsPage : ContentPage
{
    public EmergencyContactsPage() : this(MobileServiceHub.GetRequiredService<EmergencyContactsViewModel>()) { }

    public EmergencyContactsPage(EmergencyContactsViewModel viewModel)
    {
        InitializeComponent();
        BindingContext = viewModel;
    }

    protected override async void OnAppearing()
    {
        base.OnAppearing();
        if (BindingContext is EmergencyContactsViewModel vm)
            await vm.LoadAsync();
    }
}
