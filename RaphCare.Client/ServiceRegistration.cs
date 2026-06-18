using AutoMapper;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Microsoft.Extensions.DependencyInjection;
using RaphCare.Client.Contracts;
using RaphCare.Client.Contracts.Interfaces;
using RaphCare.Client.Services;
using RaphCare.Client.Services.Base;

namespace RaphCare.Client;

using ApiClient = Services.Base.Client;

public static class ServiceRegistration
{
    public const string HttpClientName = "RaphCare";

    /// <summary>Same base URL as <see cref="HttpClientName"/> but no bearer handler — for anonymous integration endpoints (e.g. standalone emergency webhook).</summary>
    public const string WebhookHttpClientName = "RaphCareWebhook";

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
        {
            services.AddTransient<BearerTokenHandler>();
            httpClientBuilder.AddHttpMessageHandler<BearerTokenHandler>();
        }

        services.TryAddSingleton<IClinicIdProvider, NullClinicIdProvider>();
        services.AddTransient<ClinicIdHeaderHandler>();
        httpClientBuilder.AddHttpMessageHandler<ClinicIdHeaderHandler>();

        services.AddHttpClient(WebhookHttpClientName, client =>
        {
            configureHttpClient?.Invoke(client);
        });

        services.AddScoped<IPatientService>(sp => new PatientService(
            sp.GetRequiredService<IClient>(),
            sp.GetRequiredService<IHttpClientFactory>().CreateClient(HttpClientName),
            sp.GetRequiredService<IMapper>()!));

        services.AddTransient<IAppointmentService>(sp => new AppointmentService(
            sp.GetRequiredService<IClient>(),
            sp.GetRequiredService<IHttpClientFactory>().CreateClient(HttpClientName)));

        services.AddTransient<IHealthRecordService>(sp => new HealthRecordService(sp.GetRequiredService<IClient>()));

        services.AddTransient<IPatientInsuranceService>(sp => new PatientInsuranceService(sp.GetRequiredService<IClient>()));

        services.AddTransient<IPatientBillingService>(sp => new PatientBillingService(
            sp.GetRequiredService<IClient>()));

        services.AddTransient<IPatientFamilyMembersService>(sp => new PatientFamilyMembersService(
            sp.GetRequiredService<IClient>()));

        services.AddTransient<IPatientMentalHealthService>(sp => new PatientMentalHealthService(
            sp.GetRequiredService<IClient>()));

        services.AddTransient<IPatientProfileService>(sp => new PatientProfileService(
            sp.GetRequiredService<IClient>()));

        services.AddTransient<IPatientNotificationsService>(sp => new PatientNotificationsService(
            sp.GetRequiredService<IClient>()));

        services.AddTransient<IPatientAiAssistantService>(sp => new PatientAiAssistantService(
            sp.GetRequiredService<IClient>()));

        services.AddTransient<IPatientTelehealthService>(sp => new PatientTelehealthService(
            sp.GetRequiredService<IClient>(),
            sp.GetRequiredService<IMapper>()!));

        services.AddTransient<IPatientDevicesService>(sp => new PatientDevicesService(
            sp.GetRequiredService<IClient>()));

        services.AddTransient<IClinicalPatientDeviceReadingsService>(sp => new ClinicalPatientDeviceReadingsService(
            sp.GetRequiredService<IClient>()));

        services.AddTransient<IStandaloneEmergencyWebhookClient>(sp => new StandaloneEmergencyWebhookClient(
            sp.GetRequiredService<IHttpClientFactory>()));

        // Transient: MAUI Shell-created pages often resolve VMs via root IServiceProvider (no scope);
        // scoped registration throws when resolved outside a scope.
        services.AddTransient<IOtpAuthService, OtpAuthService>();
        services.AddTransient<IEmailAuthService, EmailAuthService>();
        services.AddTransient<IVoiceOnboardingService, VoiceOnboardingService>();
        services.AddTransient<IAdminClinicService, AdminClinicService>();

        return services;
    }
}
