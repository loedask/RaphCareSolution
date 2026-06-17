using RaphCare.Mobile.Core.Shared.Navigation;
using RaphCare.Mobile.Resources.Strings;

namespace RaphCare.Mobile;

public partial class AppShell : Shell
{
    public AppShell()
    {
        InitializeComponent();
        AppNavigator.RegisterAllRoutes();
        ApplyTabTitles();
    }

    private void ApplyTabTitles()
    {
        if (Items.Count < 2 || Items[1] is not TabBar tabBar)
            return;

        var titles = new[]
        {
            AppResources.T("TabHome"),
            AppResources.T("TabAppointments"),
            AppResources.T("TabRecords"),
            AppResources.T("TabInsurance"),
            AppResources.T("TabProfile"),
        };

        for (var i = 0; i < tabBar.Items.Count && i < titles.Length; i++)
            tabBar.Items[i].Title = titles[i];
    }
}
