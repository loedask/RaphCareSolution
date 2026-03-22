using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using RaphCare.Client;
using RaphCare.Mobile.Core.Features.Appointments.Views;
using RaphCare.Mobile.Core.Features.Auth.ViewModels;
using RaphCare.Mobile.Core.Features.Auth.Views;
using RaphCare.Mobile.Core.Features.Home.Views;
using RaphCare.Mobile.Core.Features.Hybrid.Views;
using RaphCare.Mobile.Core.Features.Insurance.Views;
using RaphCare.Mobile.Core.Features.Records.Views;
using RaphCare.Mobile.Core.Features.Settings.Views;
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
        services.Configure<FeatureFlagOptions>(configuration.GetSection(FeatureFlagOptions.SectionName));

        services.AddSingleton<IAuthService, EntraAuthService>();
        services.AddSingleton<RaphCare.Client.Contracts.IAccessTokenProvider, SecureStorageAccessTokenProvider>();

        services.AddTransient<LandingViewModel>();
        services.AddTransient<RegisterOptionsViewModel>();
        services.AddTransient<RegisterEmailViewModel>();
        services.AddTransient<VerifyEmailViewModel>();
        services.AddTransient<SignInViewModel>();

        services.AddTransient<LandingPage>();
        services.AddTransient<RegisterOptionsPage>();
        services.AddTransient<RegisterEmailPage>();
        services.AddTransient<VerifyEmailPage>();
        services.AddTransient<SignInPage>();
        services.AddTransient<HomePage>();
        services.AddTransient<RecordsPage>();
        services.AddTransient<AppointmentsPage>();
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
