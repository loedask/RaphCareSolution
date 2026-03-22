using RaphCare.Mobile.Core.Shared.Services.FeatureFlags;
using RaphCare.Mobile.Core.Features.Appointments.Views;
using RaphCare.Mobile.Core.Features.Auth.Views;
using RaphCare.Mobile.Core.Features.Home.Views;
using RaphCare.Mobile.Core.Features.Hybrid.Views;
using RaphCare.Mobile.Core.Features.Insurance.Views;
using RaphCare.Mobile.Core.Features.Records.Views;
using RaphCare.Mobile.Core.Features.Settings.Views;
using RaphCare.Mobile.Core.Shared.Views;

namespace RaphCare.Mobile.Core.Shared.Navigation;

/// <summary>
/// Registers all app routes and provides navigation that respects feature flags.
/// </summary>
public static class AppNavigator
{
    public const string Landing = "LandingPage";
    public const string RegisterOptions = "RegisterOptionsPage";
    public const string RegisterEmail = "RegisterEmailPage";
    public const string RegisterPhone = "RegisterPhonePage";
    public const string VerifyPhone = "VerifyPhonePage";
    public const string RegisterVoiceIntro = "RegisterVoiceIntroPage";
    public const string VoiceSubmit = "VoiceSubmitPage";
    public const string VerifyEmail = "VerifyEmailPage";
    public const string SignIn = "SignInPage";
    public const string Home = "HomePage";
    public const string Records = "RecordsPage";
    public const string Appointments = "AppointmentsPage";
    public const string Insurance = "InsurancePage";
    public const string Settings = "SettingsPage";
    public const string UnderConstruction = "UnderConstructionPage";
    public const string BlazorHost = "BlazorHostPage";

    /// <summary>
    /// Call once at app startup (e.g. from AppShell or MauiProgram) to register every route.
    /// </summary>
    public static void RegisterAllRoutes()
    {
        // Auth
        Routing.RegisterRoute(Landing, typeof(LandingPage));
        Routing.RegisterRoute(RegisterOptions, typeof(RegisterOptionsPage));
        Routing.RegisterRoute(RegisterEmail, typeof(RegisterEmailPage));
        Routing.RegisterRoute(RegisterPhone, typeof(RegisterPhonePage));
        Routing.RegisterRoute(VerifyPhone, typeof(VerifyPhonePage));
        Routing.RegisterRoute(RegisterVoiceIntro, typeof(RegisterVoiceIntroPage));
        Routing.RegisterRoute(VoiceSubmit, typeof(VoiceSubmitPage));
        Routing.RegisterRoute(VerifyEmail, typeof(VerifyEmailPage));
        Routing.RegisterRoute(SignIn, typeof(SignInPage));

        // Main
        Routing.RegisterRoute(Home, typeof(HomePage));
        Routing.RegisterRoute(Records, typeof(RecordsPage));
        Routing.RegisterRoute(Appointments, typeof(AppointmentsPage));
        Routing.RegisterRoute(Insurance, typeof(InsurancePage));
        Routing.RegisterRoute(Settings, typeof(SettingsPage));

        // Shared / hybrid
        Routing.RegisterRoute(UnderConstruction, typeof(UnderConstructionPage));
        Routing.RegisterRoute(BlazorHost, typeof(BlazorHostPage));
    }

    /// <summary>
    /// Navigate to a feature page; if the feature is disabled, shows UnderConstructionPage instead.
    /// </summary>
    public static async Task GoToFeatureAsync(string route, string? featureDisplayName = null, bool absolute = false)
    {
        var (enabled, pageRoute) = GetFeatureRoute(route);
        if (enabled)
        {
            var path = absolute ? "//" + pageRoute : pageRoute;
            await SafeShellNavigator.GoToAsync(path);
        }
        else
        {
            var uri = $"{UnderConstruction}?featureName={Uri.EscapeDataString(featureDisplayName ?? route)}";
            await SafeShellNavigator.GoToAsync(uri);
        }
    }

    private static (bool enabled, string route) GetFeatureRoute(string route)
    {
        return route switch
        {
            Records => (FeatureFlags.RecordsEnabled, Records),
            Appointments => (FeatureFlags.AppointmentsEnabled, Appointments),
            Insurance => (FeatureFlags.InsuranceEnabled, Insurance),
            Settings => (FeatureFlags.SettingsEnabled, Settings),
            _ => (true, route)
        };
    }
}
