using RaphCare.Mobile.Core.Features.Appointments.ViewModels;
using RaphCare.Mobile.Core.Infrastructure.Composition;

namespace RaphCare.Mobile.Core.Features.Appointments.Views;

public partial class AppointmentsPage : ContentPage
{
    public AppointmentsPage() : this(MobileServiceHub.GetRequiredService<AppointmentsViewModel>()) { }

    public AppointmentsPage(AppointmentsViewModel viewModel)
    {
        InitializeComponent();
        BindingContext = viewModel;
    }

    protected override async void OnAppearing()
    {
        base.OnAppearing();
        if (BindingContext is AppointmentsViewModel vm)
            await vm.LoadAsync();
    }
}
