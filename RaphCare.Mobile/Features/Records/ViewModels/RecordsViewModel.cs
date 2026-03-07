using System.Windows.Input;
using RaphCare.Mobile.Core.ViewModels;
using RaphCare.Mobile.Shared.Navigation;

namespace RaphCare.Mobile.Features.Records.ViewModels;

public class RecordsViewModel : BaseViewModel
{
    public ICommand GoToRecordsCommand { get; }

    public RecordsViewModel()
    {
        Title = "Records";
        GoToRecordsCommand = new Command(async () =>
            await AppNavigator.GoToFeatureAsync(AppNavigator.Records, "Records", absolute: true));
    }
}
