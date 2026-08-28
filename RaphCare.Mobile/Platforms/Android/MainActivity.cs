using Android.App;
using Android.Content;
using Android.Content.PM;
using Android.OS;

namespace RaphCare.Mobile
{
    [Activity(Theme = "@style/Maui.SplashTheme", MainLauncher = true, ConfigurationChanges = ConfigChanges.ScreenSize | ConfigChanges.Orientation | ConfigChanges.UiMode | ConfigChanges.ScreenLayout | ConfigChanges.SmallestScreenSize | ConfigChanges.Density)]
    [IntentFilter(
        [Intent.ActionView],
        Categories = [Intent.CategoryDefault, Intent.CategoryBrowsable],
        DataScheme = "raphcare",
        DataHost = "collect")]
    public class MainActivity : MauiAppCompatActivity
    {
    }
}
