using Microsoft.Extensions.DependencyInjection.Extensions;
using Microsoft.Extensions.DependencyInjection;
using RaphCare.Client.Contracts;
using RaphCare.Client.Contracts.Interfaces;
using RaphCare.Client.Services;
using RaphCare.Client.Services.Base;

namespace RaphCare.Client;

public static class ServiceRegistration
{
    public const string HttpClientName = "RaphCare";

    /// <summary>Same base URL as <see cref="HttpClientName"/> but no bearer handler — for anonymous integration endpoints (e.g. standalone emergency webhook).</summary>
    public const string WebhookHttpClientName = "RaphCareWebhook";

    /// <summary>
    /// Registers the RaphCare API client layer: HttpClient, and feature services.
    /// Configure base address via HttpClient (e.g. from options) — do not hardcode BaseUrl.
    /// When <paramref name="useBearerToken"/> is true, ensure <see cref="Contracts.IAccessTokenProvider"/> is registered so requests include the bearer token.
    /// </summary>
    public static IServiceCollection AddRaphCareClient(this IServiceCollection services, Action<HttpClient>? configureHttpClient = null, bool useBearerToken = false)
    {
        var httpClientBuilder = services.AddHttpClient(HttpClientName, client =>
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

        services.AddTransient<IPatientService>(sp => new PatientService(
            sp.GetRequiredService<IHttpClientFactory>().CreateClient(HttpClientName)));
        services.AddTransient<IAppointmentService>(sp => new AppointmentService(
            sp.GetRequiredService<IHttpClientFactory>().CreateClient(HttpClientName)));
        services.AddTransient<IHealthRecordService>(sp => new HealthRecordService(
            sp.GetRequiredService<IHttpClientFactory>().CreateClient(HttpClientName)));
        services.AddTransient<IPatientInsuranceService>(sp => new PatientInsuranceService(
            sp.GetRequiredService<IHttpClientFactory>().CreateClient(HttpClientName)));
        services.AddTransient<IPatientBillingService>(sp => new PatientBillingService(
            sp.GetRequiredService<IHttpClientFactory>().CreateClient(HttpClientName)));
        services.AddTransient<IPatientFamilyMembersService>(sp => new PatientFamilyMembersService(
            sp.GetRequiredService<IHttpClientFactory>().CreateClient(HttpClientName)));
        services.AddTransient<IPatientMentalHealthService>(sp => new PatientMentalHealthService(
            sp.GetRequiredService<IHttpClientFactory>().CreateClient(HttpClientName)));
        services.AddTransient<IPatientProfileService>(sp => new PatientProfileService(
            sp.GetRequiredService<IHttpClientFactory>().CreateClient(HttpClientName)));
        services.AddTransient<IPatientEmergencyContactsService>(sp => new PatientEmergencyContactsService(
            sp.GetRequiredService<IHttpClientFactory>().CreateClient(HttpClientName)));
        services.AddTransient<IPatientMedicalInfoService>(sp => new PatientMedicalInfoService(
            sp.GetRequiredService<IHttpClientFactory>().CreateClient(HttpClientName)));
        services.AddTransient<IPatientNotificationsService>(sp => new PatientNotificationsService(
            sp.GetRequiredService<IHttpClientFactory>().CreateClient(HttpClientName)));
        services.AddTransient<IPatientAiAssistantService>(sp => new PatientAiAssistantService(
            sp.GetRequiredService<IHttpClientFactory>().CreateClient(HttpClientName)));
        services.AddTransient<IPatientTelehealthService>(sp => new PatientTelehealthService(
            sp.GetRequiredService<IHttpClientFactory>().CreateClient(HttpClientName)));
        services.AddTransient<IPatientDevicesService>(sp => new PatientDevicesService(
            sp.GetRequiredService<IHttpClientFactory>().CreateClient(HttpClientName)));
        services.AddTransient<IClinicalPatientDeviceReadingsService>(sp => new ClinicalPatientDeviceReadingsService(
            sp.GetRequiredService<IHttpClientFactory>().CreateClient(HttpClientName)));

        services.AddTransient<IStandaloneEmergencyWebhookClient, StandaloneEmergencyWebhookClient>();
        services.AddTransient<IOtpAuthService, OtpAuthService>();
        services.AddTransient<IEmailAuthService, EmailAuthService>();
        services.AddTransient<IVoiceOnboardingService, VoiceOnboardingService>();
        services.AddTransient<IAdminClinicService, AdminClinicService>();

        return services;
    }
}
