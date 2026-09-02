using RaphCare.Mobile.Core.Common.Navigation;
using RaphCare.Mobile.Core.Features.Appointments.ViewModels;
using RaphCare.Mobile.Core.Infrastructure.Composition;

namespace RaphCare.Mobile.Core.Features.Appointments.Views;

public partial class BookAppointmentPage : ContentPage
{
    public BookAppointmentPage() : this(MobileServiceHub.GetRequiredService<BookAppointmentViewModel>()) { }

    public BookAppointmentPage(BookAppointmentViewModel viewModel)
    {
        InitializeComponent();
        BindingContext = viewModel;
    }

    protected override async void OnAppearing()
    {
        base.OnAppearing();
        if (BindingContext is BookAppointmentViewModel vm)
            await SafePageLoad.RunAsync(() => vm.LoadAsync());
    }
}
