using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using RaphCare.Client;
using RaphCare.Mobile.Core.Features.Appointments.ViewModels;
using RaphCare.Mobile.Core.Features.Appointments.Views;
using RaphCare.Mobile.Core.Features.Auth.ViewModels;
using RaphCare.Mobile.Core.Features.Auth.Views;
using RaphCare.Mobile.Core.Features.Home.ViewModels;
using RaphCare.Mobile.Core.Features.Home.Views;
using RaphCare.Mobile.Core.Features.Hybrid.Views;
using RaphCare.Mobile.Core.Features.Insurance.Views;
using RaphCare.Mobile.Core.Features.Records.ViewModels;
using RaphCare.Mobile.Core.Features.Records.Views;
using RaphCare.Mobile.Core.Features.Settings.Views;
using RaphCare.Mobile.Core.Shared.Configuration;
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
        services.Configure<OnboardingOptions>(configuration.GetSection(OnboardingOptions.SectionName));
        services.Configure<AppointmentsMobileOptions>(configuration.GetSection(AppointmentsMobileOptions.SectionName));
        services.Configure<FeatureFlagOptions>(configuration.GetSection(FeatureFlagOptions.SectionName));

        services.AddSingleton<IAuthService, EntraAuthService>();
        services.AddSingleton<RaphCare.Client.Contracts.IAccessTokenProvider, SecureStorageAccessTokenProvider>();

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
        services.AddTransient<SettingsPage>();
        services.AddTransient<UnderConstructionPage>();
        services.AddTransient<BlazorHostPage>();
        services.AddTransient<AppShell>();

        var apiBaseAddress = configuration["Api:BaseAddress"] ?? "https://localhost:7001/";
        services.AddRaphCareClient(client =>
        {
            client.BaseAddress = new Uri(apiBaseAddress);
        }, useBearerToken: true);

        return services;
    }
}
