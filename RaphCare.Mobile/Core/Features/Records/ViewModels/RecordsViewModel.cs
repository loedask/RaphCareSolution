using System.Windows.Input;
using RaphCare.Mobile.Core.Shared.Navigation;
using RaphCare.Mobile.Core.ViewModels;

namespace RaphCare.Mobile.Core.Features.Records.ViewModels;

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
