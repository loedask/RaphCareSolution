using System.Windows.Input;
using RaphCare.Mobile.Core.ViewModels;
using RaphCare.Mobile.Shared.Navigation;

namespace RaphCare.Mobile.Core.Features.Appointments.ViewModels;

public class AppointmentsViewModel : BaseViewModel
{
    public ICommand GoToAppointmentsCommand { get; }

    public AppointmentsViewModel()
    {
        Title = "Appointments";
        GoToAppointmentsCommand = new Command(async () =>
            await AppNavigator.GoToFeatureAsync(AppNavigator.Appointments, "Appointments", absolute: true));
    }
}
