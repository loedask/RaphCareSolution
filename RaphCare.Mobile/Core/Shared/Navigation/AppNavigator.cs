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
    public const string AccountCreated = "AccountCreatedPage";
    public const string SignIn = "SignInPage";
    public const string Home = "HomePage";
    public const string Records = "RecordsPage";
    public const string HealthRecordDetail = "HealthRecordDetailPage";
    public const string Appointments = "AppointmentsPage";
    public const string AppointmentDetail = "AppointmentDetailPage";
    public const string BookAppointment = "BookAppointmentPage";
    public const string Insurance = "InsurancePage";
    public const string Settings = "SettingsPage";
    public const string UnderConstruction = "UnderConstructionPage";

    /// <summary>Care: request call, consultation (concept /request-call, /consultation). Stub until vertical ships.</summary>
    public const string CareTelehealth = "CareTelehealthPage";

    /// <summary>Connected devices / BLE wearables (concept /devices). Stub until vertical ships.</summary>
    public const string Devices = "DevicesPage";

    /// <summary>Billing & plans (concept payment / billing / upgrade). Stub until vertical ships.</summary>
    public const string Billing = "BillingPage";

    public const string MentalHealth = "MentalHealthPage";
    public const string FamilyMembers = "FamilyMembersPage";
    public const string AiAssistant = "AiAssistantPage";
    public const string Notifications = "NotificationsPage";
    public const string BlazorHost = "BlazorHostPage";

    /// <summary>
    /// Routes that resolve to <see cref="UnderConstructionPage"/> with a display name until the vertical is implemented.
    /// Remove a route from this set when replacing registration with a real page type.
    /// </summary>
    private static readonly HashSet<string> StubRoutes =
    [
        CareTelehealth,
        Devices,
        Billing,
        MentalHealth,
        FamilyMembers,
        AiAssistant,
        Notifications,
    ];

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
        Routing.RegisterRoute(AccountCreated, typeof(AccountCreatedPage));
        Routing.RegisterRoute(SignIn, typeof(SignInPage));

        // Main
        Routing.RegisterRoute(Home, typeof(HomePage));
        Routing.RegisterRoute(Records, typeof(RecordsPage));
        Routing.RegisterRoute(HealthRecordDetail, typeof(HealthRecordDetailPage));
        Routing.RegisterRoute(Appointments, typeof(AppointmentsPage));
        Routing.RegisterRoute(AppointmentDetail, typeof(AppointmentDetailPage));
        Routing.RegisterRoute(BookAppointment, typeof(BookAppointmentPage));
        Routing.RegisterRoute(Insurance, typeof(InsurancePage));
        Routing.RegisterRoute(Settings, typeof(SettingsPage));

        // Concept areas (stubs → same page type; query <c>featureName</c> set by <see cref="GoToFeatureAsync"/> when flag is on)
        Routing.RegisterRoute(CareTelehealth, typeof(UnderConstructionPage));
        Routing.RegisterRoute(Devices, typeof(UnderConstructionPage));
        Routing.RegisterRoute(Billing, typeof(UnderConstructionPage));
        Routing.RegisterRoute(MentalHealth, typeof(UnderConstructionPage));
        Routing.RegisterRoute(FamilyMembers, typeof(UnderConstructionPage));
        Routing.RegisterRoute(AiAssistant, typeof(UnderConstructionPage));
        Routing.RegisterRoute(Notifications, typeof(UnderConstructionPage));

        Routing.RegisterRoute(UnderConstruction, typeof(UnderConstructionPage));
        Routing.RegisterRoute(BlazorHost, typeof(BlazorHostPage));
    }

    /// <summary>
    /// Navigate to a feature page; if the feature is disabled, shows UnderConstructionPage instead.
    /// </summary>
    public static async Task GoToFeatureAsync(string route, string? featureDisplayName = null, bool absolute = false)
    {
        var display = featureDisplayName ?? route;
        var (enabled, pageRoute) = GetFeatureRoute(route);
        if (!enabled)
        {
            var uri = $"{UnderConstruction}?featureName={Uri.EscapeDataString(display)}";
            await SafeShellNavigator.GoToAsync(uri);
            return;
        }

        if (StubRoutes.Contains(pageRoute))
        {
            var q = $"featureName={Uri.EscapeDataString(display)}";
            var path = absolute ? $"//{pageRoute}?{q}" : $"{pageRoute}?{q}";
            await SafeShellNavigator.GoToAsync(path);
            return;
        }

        var normalPath = absolute ? "//" + pageRoute : pageRoute;
        await SafeShellNavigator.GoToAsync(normalPath);
    }

    private static (bool enabled, string route) GetFeatureRoute(string route)
    {
        return route switch
        {
            Records => (FeatureFlags.RecordsEnabled, Records),
            Appointments => (FeatureFlags.AppointmentsEnabled, Appointments),
            Insurance => (FeatureFlags.InsuranceEnabled, Insurance),
            Settings => (FeatureFlags.SettingsEnabled, Settings),
            CareTelehealth => (FeatureFlags.CareTelehealthEnabled, CareTelehealth),
            Devices => (FeatureFlags.DevicesEnabled, Devices),
            Billing => (FeatureFlags.BillingEnabled, Billing),
            MentalHealth => (FeatureFlags.MentalHealthEnabled, MentalHealth),
            FamilyMembers => (FeatureFlags.FamilyMembersEnabled, FamilyMembers),
            AiAssistant => (FeatureFlags.AiAssistantEnabled, AiAssistant),
            Notifications => (FeatureFlags.NotificationsEnabled, Notifications),
            _ => (true, route)
        };
    }
}
