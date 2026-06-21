using System.Globalization;
using RaphCare.Mobile.Core.Common.Navigation;
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
            AppResources.T("TabHome", CultureInfo.CurrentUICulture),
            AppResources.T("TabAppointments", CultureInfo.CurrentUICulture),
            AppResources.T("TabRecords", CultureInfo.CurrentUICulture),
            AppResources.T("TabInsurance", CultureInfo.CurrentUICulture),
            AppResources.T("TabProfile", CultureInfo.CurrentUICulture),
        };

        for (var i = 0; i < tabBar.Items.Count && i < titles.Length; i++)
            tabBar.Items[i].Title = titles[i];
    }
}
