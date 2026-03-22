using System.Windows.Input;
using RaphCare.Mobile.Core.Shared.Navigation;
using RaphCare.Mobile.Core.Shared.ViewModels;

namespace RaphCare.Mobile.Core.Features.Insurance.ViewModels;

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
