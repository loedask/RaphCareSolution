using System.Windows.Input;
using RaphCare.Mobile.Core.ViewModels;
using RaphCare.Mobile.Shared.Navigation;

namespace RaphCare.Mobile.Features.Insurance.ViewModels;

public class InsuranceViewModel : BaseViewModel
{
    public ICommand GoToInsuranceCommand { get; }

    public InsuranceViewModel()
    {
        Title = "Insurance";
        GoToInsuranceCommand = new Command(async () =>
            await AppNavigator.GoToFeatureAsync(AppNavigator.Insurance, "Insurance", absolute: true));
    }
}
