using System.Windows.Input;
using RaphCare.Mobile.Core.Shared.Navigation;
using RaphCare.Mobile.Core.Shared.ViewModels;

namespace RaphCare.Mobile.Core.Features.Settings.ViewModels;

public class SettingsViewModel : BaseViewModel
{
    public ICommand GoToSettingsCommand { get; }

    public SettingsViewModel()
    {
        Title = "Settings";
        GoToSettingsCommand = new Command(async () =>
            await AppNavigator.GoToFeatureAsync(AppNavigator.Settings, "Settings", absolute: true));
    }
}
