using AutoMapper;
using Microsoft.Extensions.DependencyInjection;
using RaphCare.Client.Contracts.Interfaces;
using RaphCare.Client.Services;
using RaphCare.Client.Services.Base;

namespace RaphCare.Client;

using ApiClient = Services.Base.Client;

public static class ServiceRegistration
{
    public const string HttpClientName = "RaphCare";

    /// <summary>
    /// Registers the RaphCare API client layer: NSwag client, HttpClient, AutoMapper, and feature services.
    /// Configure base address via HttpClient (e.g. from options) — do not hardcode BaseUrl.
    /// When <paramref name="useBearerToken"/> is true, ensure <see cref="Contracts.IAccessTokenProvider"/> is registered so requests include the bearer token.
    /// </summary>
    public static IServiceCollection AddRaphCareClient(this IServiceCollection services, Action<HttpClient>? configureHttpClient = null, bool useBearerToken = false)
    {
        services.AddAutoMapper(cfg =>
        {
            cfg.AddMaps(typeof(ServiceRegistration).Assembly);
        });

        var httpClientBuilder = services.AddHttpClient<IClient, ApiClient>(HttpClientName, client =>
        {
            configureHttpClient?.Invoke(client);
        });

        if (useBearerToken)
            httpClientBuilder.AddHttpMessageHandler<BearerTokenHandler>();

        services.AddScoped<IPatientService>(sp => new PatientService(
            sp.GetRequiredService<IClient>(),
            sp.GetRequiredService<IHttpClientFactory>().CreateClient(HttpClientName),
            sp.GetRequiredService<IMapper>()!));

        services.AddTransient<IAppointmentService>(sp => new AppointmentService(
            sp.GetRequiredService<IClient>(),
            sp.GetRequiredService<IHttpClientFactory>().CreateClient(HttpClientName)));

        services.AddTransient<IHealthRecordService>(sp => new HealthRecordService(sp.GetRequiredService<IClient>()));

        services.AddTransient<IPatientInsuranceService>(sp => new PatientInsuranceService(
            sp.GetRequiredService<IClient>(),
            sp.GetRequiredService<IHttpClientFactory>().CreateClient(HttpClientName)));

        // Transient: MAUI Shell-created pages often resolve VMs via root IServiceProvider (no scope);
        // scoped registration throws when resolved outside a scope.
        services.AddTransient<IOtpAuthService, OtpAuthService>();
        services.AddTransient<IVoiceOnboardingService, VoiceOnboardingService>();

        return services;
    }
}
