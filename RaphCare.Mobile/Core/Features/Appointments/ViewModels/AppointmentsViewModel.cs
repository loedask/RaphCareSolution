using System.Windows.Input;
using RaphCare.Mobile.Core.Shared.Navigation;
using RaphCare.Mobile.Core.Shared.ViewModels;

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
