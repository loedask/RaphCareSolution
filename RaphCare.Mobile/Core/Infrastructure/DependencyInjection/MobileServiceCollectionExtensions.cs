using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using RaphCare.Client;
using RaphCare.Client.Contracts.Interfaces;
using RaphCare.Mobile.Core.Features.Appointments.ViewModels;
using RaphCare.Mobile.Core.Features.Appointments.Views;
using RaphCare.Mobile.Core.Features.Auth.ViewModels;
using RaphCare.Mobile.Core.Features.Auth.Views;
using RaphCare.Mobile.Core.Features.CareTelehealth.Rtc;
using RaphCare.Mobile.Core.Features.CareTelehealth.ViewModels;
using RaphCare.Mobile.Core.Features.CareTelehealth.Views;
using RaphCare.Mobile.Core.Features.Devices.Services;
using RaphCare.Mobile.Core.Features.Devices.ViewModels;
using RaphCare.Mobile.Core.Features.Devices.Views;
using RaphCare.Mobile.Core.Features.Billing.ViewModels;
using RaphCare.Mobile.Core.Features.Billing.Views;
using RaphCare.Mobile.Core.Features.Family.ViewModels;
using RaphCare.Mobile.Core.Features.Family.Views;
using RaphCare.Mobile.Core.Features.MentalHealth.ViewModels;
using RaphCare.Mobile.Core.Features.MentalHealth.Views;
using RaphCare.Mobile.Core.Features.Notifications.ViewModels;
using RaphCare.Mobile.Core.Features.Notifications.Views;
using RaphCare.Mobile.Core.Features.AiAssistant.ViewModels;
using RaphCare.Mobile.Core.Features.AiAssistant.Views;
#if ANDROID
using RaphCare.Mobile.Platforms.Android.Telehealth;
#endif
using RaphCare.Mobile.Core.Features.Home.ViewModels;
using RaphCare.Mobile.Core.Features.Home.Views;
using RaphCare.Mobile.Core.Features.Hybrid.Views;
using RaphCare.Mobile.Core.Features.Insurance.ViewModels;
using RaphCare.Mobile.Core.Features.Insurance.Views;
using RaphCare.Mobile.Core.Features.Records.ViewModels;
using RaphCare.Mobile.Core.Features.Records.Views;
using RaphCare.Mobile.Core.Features.Settings.Services;
using RaphCare.Mobile.Core.Features.Settings.ViewModels;
using RaphCare.Mobile.Core.Features.Settings.Views;
using RaphCare.Client.Contracts;
using RaphCare.Mobile.Core.Shared.Configuration;
using RaphCare.Mobile.Core.Shared.Services.Api;
using RaphCare.Mobile.Core.Shared.Services.Auth;
using RaphCare.Mobile.Core.Shared.Services.FeatureFlags;
using RaphCare.Mobile.Core.Shared.Views;

namespace RaphCare.Mobile.Core.Infrastructure.DependencyInjection;

public static class MobileServiceCollectionExtensions
{
    /// <summary>
    /// Registers mobile app services: Entra auth, feature flags options, view models, Shell pages, API client.
    /// </summary>
    public static IServiceCollection AddRaphCareMobile(this IServiceCollection services, IConfiguration configuration)
    {
        services.Configure<EntraAuthOptions>(configuration.GetSection(EntraAuthOptions.SectionName));
        services.Configure<ApiMobileOptions>(configuration.GetSection(ApiMobileOptions.SectionName));
        services.Configure<OnboardingOptions>(configuration.GetSection(OnboardingOptions.SectionName));
        services.Configure<AppointmentsMobileOptions>(configuration.GetSection(AppointmentsMobileOptions.SectionName));
        services.Configure<FeatureFlagOptions>(configuration.GetSection(FeatureFlagOptions.SectionName));

        services.AddSingleton<IAuthService, EntraAuthService>();
        services.AddSingleton<IClinicIdProvider, ConfigurationClinicIdProvider>();
        services.AddSingleton<RaphCare.Client.Contracts.IAccessTokenProvider, SecureStorageAccessTokenProvider>();

#if ANDROID
        services.AddSingleton<ITelehealthRtcSession, AgoraAndroidTelehealthRtcSession>();
#else
        services.AddSingleton<ITelehealthRtcSession, NoOpTelehealthRtcSession>();
#endif

        services.AddSingleton<IWearableBleCoordinator, WearableBleCoordinator>();

        services.AddSingleton<IVitalsSyncOutbox, FileVitalsSyncOutbox>();

        services.AddSingleton<ILocalPatientProfileStore, LocalPatientProfileStore>();

        services.AddTransient<LandingViewModel>();
        services.AddTransient<RegisterOptionsViewModel>();
        services.AddTransient<RegisterEmailViewModel>();
        services.AddTransient<RegisterPhoneViewModel>();
        services.AddTransient<VerifyPhoneViewModel>();
        services.AddTransient<RegisterVoiceIntroViewModel>();
        services.AddTransient<VoiceSubmitViewModel>();
        services.AddTransient<VerifyEmailViewModel>();
        services.AddTransient<AccountCreatedViewModel>();
        services.AddTransient<SignInViewModel>();
        services.AddTransient<HomeViewModel>();
        services.AddTransient<AppointmentsViewModel>();
        services.AddTransient<AppointmentDetailViewModel>();
        services.AddTransient<BookAppointmentViewModel>();
        services.AddTransient<RecordsViewModel>();
        services.AddTransient<HealthRecordDetailViewModel>();
        services.AddTransient<InsuranceViewModel>();
        services.AddTransient<AddInsuranceProfileViewModel>();
        services.AddTransient<InsuranceProfileDetailViewModel>();
        services.AddTransient<CareTelehealthViewModel>();
        services.AddTransient<TelehealthJoinViewModel>(sp => new TelehealthJoinViewModel(
            sp.GetRequiredService<IPatientTelehealthService>(),
            sp.GetRequiredService<ITelehealthRtcSession>()));

        services.AddTransient<LandingPage>();
        services.AddTransient<RegisterOptionsPage>();
        services.AddTransient<RegisterEmailPage>();
        services.AddTransient<RegisterPhonePage>();
        services.AddTransient<VerifyPhonePage>();
        services.AddTransient<RegisterVoiceIntroPage>();
        services.AddTransient<VoiceSubmitPage>();
        services.AddTransient<VerifyEmailPage>();
        services.AddTransient<AccountCreatedPage>();
        services.AddTransient<SignInPage>();
        services.AddTransient<HomePage>();
        services.AddTransient<RecordsPage>();
        services.AddTransient<HealthRecordDetailPage>();
        services.AddTransient<AppointmentsPage>();
        services.AddTransient<AppointmentDetailPage>();
        services.AddTransient<BookAppointmentPage>();
        services.AddTransient<InsurancePage>();
        services.AddTransient<InsuranceProfileDetailPage>();
        services.AddTransient<AddInsuranceProfilePage>();
        services.AddTransient<CareTelehealthPage>();
        services.AddTransient<TelehealthJoinPage>();
        services.AddTransient<DevicesViewModel>();
        services.AddTransient<DevicesPage>();
        services.AddTransient<BillingViewModel>();
        services.AddTransient<AddPaymentMethodViewModel>();
        services.AddTransient<BillingPage>();
        services.AddTransient<AddPaymentMethodPage>();
        services.AddTransient<FamilyMembersViewModel>();
        services.AddTransient<AddFamilyMemberViewModel>();
        services.AddTransient<FamilyMemberDetailViewModel>();
        services.AddTransient<FamilyMembersPage>();
        services.AddTransient<AddFamilyMemberPage>();
        services.AddTransient<FamilyMemberDetailPage>();
        services.AddTransient<MentalHealthViewModel>();
        services.AddTransient<MentalHealthPage>();
        services.AddTransient<NotificationsViewModel>();
        services.AddTransient<NotificationsPage>();
        services.AddTransient<AiAssistantViewModel>();
        services.AddTransient<AiAssistantPage>();
        services.AddTransient<ProfileHubViewModel>();
        services.AddTransient<EditProfileViewModel>();
        services.AddTransient<PrivacySettingsViewModel>();
        services.AddTransient<HelpSupportViewModel>();
        services.AddTransient<SettingsPage>();
        services.AddTransient<EditProfilePage>();
        services.AddTransient<PrivacyPage>();
        services.AddTransient<HelpSupportPage>();
        services.AddTransient<UnderConstructionPage>();
        services.AddTransient<BlazorHostPage>();
        services.AddTransient<AppShell>();

        var apiBaseAddress = ResolveApiBaseAddress(configuration["Api:BaseAddress"]);
        services.AddRaphCareClient(client =>
        {
            client.BaseAddress = new Uri(apiBaseAddress);
        }, useBearerToken: true);

        return services;
    }

    private static string ResolveApiBaseAddress(string? configured)
    {
        var address = string.IsNullOrWhiteSpace(configured) ? "http://localhost:5281/" : configured.Trim();

#if DEBUG && ANDROID
        // Emulator: localhost is the device. Local ASP.NET dev HTTPS certs are not trusted on Android.
        if (Uri.TryCreate(address, UriKind.Absolute, out var uri)
            && (uri.Host.Equals("localhost", StringComparison.OrdinalIgnoreCase)
                || uri.Host.Equals("10.0.2.2", StringComparison.OrdinalIgnoreCase)))
        {
            return "http://10.0.2.2:5281/";
        }
#endif

        return address.EndsWith('/') ? address : address + "/";
    }
}
