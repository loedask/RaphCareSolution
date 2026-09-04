using Microsoft.Maui.Controls;
using RaphCare.Mobile.Core.Features.Appointments.ViewModels;
using RaphCare.Mobile.Core.Infrastructure.Composition;
using RaphCare.Mobile.Core.Common.Navigation;

namespace RaphCare.Mobile.Core.Features.Appointments.Views;

public partial class AppointmentDetailPage : ContentPage, IQueryAttributable
{
    public AppointmentDetailPage() : this(MobileServiceHub.GetRequiredService<AppointmentDetailViewModel>()) { }

    public AppointmentDetailPage(AppointmentDetailViewModel viewModel)
    {
        InitializeComponent();
        BindingContext = viewModel;
    }

    public void ApplyQueryAttributes(IDictionary<string, object> query)
    {
        if (BindingContext is AppointmentDetailViewModel vm)
            vm.ApplyQueryAttributes(query);
    }

    protected override async void OnAppearing()
    {
        base.OnAppearing();
        if (BindingContext is AppointmentDetailViewModel vm)
            await SafePageLoad.RunAsync(() => vm.LoadAsync());
    }
}
