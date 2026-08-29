using RaphCare.Mobile.Core.Common.Services.FeatureFlags;
using RaphCare.Mobile.Core.Features.Appointments.Views;
using RaphCare.Mobile.Core.Features.Auth.Views;
using RaphCare.Mobile.Core.Features.Home.Views;
using RaphCare.Mobile.Core.Features.Hybrid.Views;
using RaphCare.Mobile.Core.Features.CareTelehealth.Views;
using RaphCare.Mobile.Core.Features.Insurance.Views;
using RaphCare.Mobile.Core.Features.Records.Views;
using RaphCare.Mobile.Core.Features.Settings.Views;
using RaphCare.Mobile.Core.Features.Devices.Views;
using RaphCare.Mobile.Core.Features.Billing.Views;
using RaphCare.Mobile.Core.Features.Family.Views;
using RaphCare.Mobile.Core.Features.MentalHealth.Views;
using RaphCare.Mobile.Core.Features.Notifications.Views;
using RaphCare.Mobile.Core.Features.AiAssistant.Views;
using RaphCare.Mobile.Core.Common.Views;

namespace RaphCare.Mobile.Core.Common.Navigation;

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
    public const string ForgotPassword = "ForgotPasswordPage";
    public const string Home = "HomePage";
    public const string Records = "RecordsPage";
    public const string HealthRecordDetail = "HealthRecordDetailPage";
    public const string Appointments = "AppointmentsPage";
    public const string AppointmentDetail = "AppointmentDetailPage";
    public const string BookAppointment = "BookAppointmentPage";
    public const string Insurance = "InsurancePage";
    public const string InsuranceProfileDetail = "InsuranceProfileDetailPage";
    public const string AddInsuranceProfile = "AddInsuranceProfilePage";
    public const string Settings = "SettingsPage";

    /// <summary>Profile hub (concept <c>/profile</c>).</summary>
    public const string EditProfile = "EditProfilePage";

    /// <summary>Privacy &amp; data (concept <c>/privacy</c>).</summary>
    public const string Privacy = "PrivacyPage";

    /// <summary>Help &amp; support (concept <c>/help-support</c>).</summary>
    public const string HelpSupport = "HelpSupportPage";

    public const string HelpFaq = "HelpFaqPage";
    public const string SupportMessage = "SupportMessagePage";

    public const string PersonalInformation = "PersonalInformationPage";
    public const string ChangePassword = "ChangePasswordPage";
    public const string LanguageSettings = "LanguageSettingsPage";
    public const string SelectClinic = "SelectClinicPage";
    public const string MedicalInformation = "MedicalInformationPage";
    public const string EmergencyContacts = "EmergencyContactsPage";
    public const string RequestCall = "RequestCallPage";

    public const string UnderConstruction = "UnderConstructionPage";

    /// <summary>Care and telehealth: session list (Agora join info + Twilio SMS from API).</summary>
    public const string CareTelehealth = "CareTelehealthPage";

    /// <summary>Telehealth join details for a session (query: <c>sessionId</c>).</summary>
    public const string TelehealthJoin = "TelehealthJoinPage";

    /// <summary>Connected devices and BLE wearables (E580/E585-class scan and connect).</summary>
    public const string Devices = "DevicesPage";

    /// <summary>Billing and plans (payment methods, invoices, plan upgrade).</summary>
    public const string Billing = "BillingPage";

    /// <summary>Add a saved payment method (demo-style; not card tokenization).</summary>
    public const string AddBillingPaymentMethod = "AddBillingPaymentMethodPage";

    public const string MentalHealth = "MentalHealthPage";

    /// <summary>Family members (concept <c>/family-members</c>).</summary>
    public const string FamilyMembers = "FamilyMembersPage";

    public const string AddFamilyMember = "AddFamilyMemberPage";

    /// <summary>Query: <c>memberId</c>.</summary>
    public const string FamilyMemberDetail = "FamilyMemberDetailPage";
    public const string AiAssistant = "AiAssistantPage";
    public const string Notifications = "NotificationsPage";
    public const string BlazorHost = "BlazorHostPage";

    /// <summary>
    /// Routes that resolve to <see cref="UnderConstructionPage"/> with a display name until the vertical is implemented.
    /// Remove a route from this set when replacing registration with a real page type.
    /// </summary>
    private static readonly HashSet<string> StubRoutes = [];

    /// <summary>
    /// Call once at app startup (e.g. from AppShell or MauiProgram) to register every route.
    /// Pages already declared in <c>AppShell.xaml</c> (Landing + main tabs) must not be registered here — duplicate routes cause Shell "ambiguous routes" crashes.
    /// </summary>
    public static void RegisterAllRoutes()
    {
        // Auth (not in Shell visual tree)
        Routing.RegisterRoute(RegisterOptions, typeof(RegisterOptionsPage));
        Routing.RegisterRoute(RegisterEmail, typeof(RegisterEmailPage));
        Routing.RegisterRoute(RegisterPhone, typeof(RegisterPhonePage));
        Routing.RegisterRoute(VerifyPhone, typeof(VerifyPhonePage));
        Routing.RegisterRoute(RegisterVoiceIntro, typeof(RegisterVoiceIntroPage));
        Routing.RegisterRoute(VoiceSubmit, typeof(VoiceSubmitPage));
        Routing.RegisterRoute(VerifyEmail, typeof(VerifyEmailPage));
        Routing.RegisterRoute(AccountCreated, typeof(AccountCreatedPage));
        Routing.RegisterRoute(SignIn, typeof(SignInPage));
        Routing.RegisterRoute(ForgotPassword, typeof(ForgotPasswordPage));

        // Feature pages (pushed from tabs or deep links — not ShellContent routes)
        Routing.RegisterRoute(HealthRecordDetail, typeof(HealthRecordDetailPage));
        Routing.RegisterRoute(AppointmentDetail, typeof(AppointmentDetailPage));
        Routing.RegisterRoute(BookAppointment, typeof(BookAppointmentPage));
        Routing.RegisterRoute(InsuranceProfileDetail, typeof(InsuranceProfileDetailPage));
        Routing.RegisterRoute(AddInsuranceProfile, typeof(AddInsuranceProfilePage));
        Routing.RegisterRoute(EditProfile, typeof(EditProfilePage));
        Routing.RegisterRoute(Privacy, typeof(PrivacyPage));
        Routing.RegisterRoute(HelpSupport, typeof(HelpSupportPage));
        Routing.RegisterRoute(HelpFaq, typeof(HelpFaqPage));
        Routing.RegisterRoute(SupportMessage, typeof(SupportMessagePage));
        Routing.RegisterRoute(PersonalInformation, typeof(PersonalInformationPage));
        Routing.RegisterRoute(ChangePassword, typeof(ChangePasswordPage));
        Routing.RegisterRoute(LanguageSettings, typeof(LanguageSettingsPage));
        Routing.RegisterRoute(SelectClinic, typeof(SelectClinicPage));
        Routing.RegisterRoute(MedicalInformation, typeof(MedicalInformationPage));
        Routing.RegisterRoute(EmergencyContacts, typeof(EmergencyContactsPage));

        Routing.RegisterRoute(CareTelehealth, typeof(CareTelehealthPage));
        Routing.RegisterRoute(RequestCall, typeof(RequestCallPage));
        Routing.RegisterRoute(TelehealthJoin, typeof(TelehealthJoinPage));

        Routing.RegisterRoute(Devices, typeof(DevicesPage));

        Routing.RegisterRoute(Billing, typeof(BillingPage));
        Routing.RegisterRoute(AddBillingPaymentMethod, typeof(AddPaymentMethodPage));

        Routing.RegisterRoute(FamilyMembers, typeof(FamilyMembersPage));
        Routing.RegisterRoute(AddFamilyMember, typeof(AddFamilyMemberPage));
        Routing.RegisterRoute(FamilyMemberDetail, typeof(FamilyMemberDetailPage));

        // Concept areas (stubs → same page type; query <c>featureName</c> set by <see cref="GoToFeatureAsync"/> when flag is on)
        Routing.RegisterRoute(MentalHealth, typeof(MentalHealthPage));
        Routing.RegisterRoute(AiAssistant, typeof(AiAssistantPage));
        Routing.RegisterRoute(Notifications, typeof(NotificationsPage));

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
            RequestCall => (FeatureFlags.CareTelehealthEnabled, RequestCall),
            RegisterPhone => (FeatureFlags.PhoneRegistrationEnabled, RegisterPhone),
            RegisterVoiceIntro => (FeatureFlags.VoiceRegistrationEnabled, RegisterVoiceIntro),
            _ => (true, route)
        };
    }
}
