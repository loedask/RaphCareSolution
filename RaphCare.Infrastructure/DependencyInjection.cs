using System.Net.Http.Headers;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Options;
using RaphCare.Application.Common.Configuration;
using RaphCare.Application.Common.Interfaces;
using RaphCare.Infrastructure.Configuration;
using RaphCare.Infrastructure.Persistence;
using RaphCare.Infrastructure.Persistence.Interceptors;
using RaphCare.Infrastructure.Services;
using RaphCare.Infrastructure.StandaloneEmergency;
using RaphCare.Infrastructure.Storage;
using RaphCare.Infrastructure.Telehealth;
using RaphCare.Infrastructure.Notifications;

namespace RaphCare.Infrastructure;

/// <summary>Infrastructure layer registration: domain event dispatcher, EF Core interceptors, and external service implementations (email, SMS, payment, AI, device, tele-session).</summary>
public static class DependencyInjection
{
    /// <summary>Registers infrastructure services. Call before AddPersistence so interceptors are available for DbContext registration.</summary>
    /// <param name="services">The service collection.</param>
    /// <param name="configuration">Application configuration (optional for future use).</param>
    /// <returns>The service collection for chaining.</returns>
    public static IServiceCollection AddInfrastructure(
        this IServiceCollection services,
        IConfiguration configuration,
        IHostEnvironment? environment = null)
    {
        services.AddScoped<IDomainEventDispatcher, DomainEventDispatcher>();
        services.AddScoped<AuditableEntityInterceptor>();
        services.AddScoped<SoftDeleteInterceptor>();
        services.AddScoped<DomainEventDispatcherInterceptor>();

        services.AddScoped<ICurrentUserService, CurrentUserService>();
        services.AddScoped<IDateTimeProvider, DateTimeProvider>();

        services.Configure<SmtpOptions>(configuration.GetSection(SmtpOptions.SectionName));
        services.Configure<RaphCarePortalOptions>(configuration.GetSection(RaphCarePortalOptions.SectionName));
        var smtp = configuration.GetSection(SmtpOptions.SectionName).Get<SmtpOptions>();
        if (smtp?.IsEnabled == true)
            services.AddScoped<IEmailService, SmtpEmailService>();
        else
            services.AddScoped<IEmailService, DevelopmentEmailService>();

        services.Configure<StandaloneEmergencyOptions>(configuration.GetSection(StandaloneEmergencyOptions.SectionName));
        services.AddSingleton<IStandaloneEmergencyWebhookSignatureValidator, StandaloneEmergencyWebhookSignatureValidator>();

        services.Configure<TwilioSmsOptions>(configuration.GetSection(TwilioSmsOptions.SectionName));
        // One ISmsService for the API: OTP, telehealth reminders, and any future SMS. Twilio when fully configured.
        var twilio = configuration.GetSection(TwilioSmsOptions.SectionName).Get<TwilioSmsOptions>();
        if (twilio?.IsEnabled == true)
            services.AddScoped<ISmsService, TwilioSmsService>();
        else
            services.AddScoped<ISmsService, SmsService>();

        services.Configure<AgoraRtcOptions>(configuration.GetSection(AgoraRtcOptions.SectionName));
        services.AddSingleton<ITelehealthRtcTokenGenerator, AgoraRtcTokenService>();
        services.Configure<JwtOptions>(configuration.GetSection(JwtOptions.SectionName));
        services.AddScoped<ITokenService, TokenService>();
        services.Configure<PaystackOptions>(configuration.GetSection(PaystackOptions.SectionName));
        var paystack = configuration.GetSection(PaystackOptions.SectionName).Get<PaystackOptions>();
        if (paystack?.IsEnabled == true)
        {
            services.AddHttpClient(PaystackPaymentGatewayService.HttpClientName, (sp, client) =>
            {
                var opts = sp.GetRequiredService<IOptions<PaystackOptions>>().Value;
                client.BaseAddress = new Uri("https://api.paystack.co/");
                client.DefaultRequestHeaders.Authorization =
                    new AuthenticationHeaderValue("Bearer", opts.SecretKey);
            });
            services.AddScoped<IPaymentGatewayService, PaystackPaymentGatewayService>();
        }
        else
            services.AddScoped<IPaymentGatewayService, PaymentGatewayService>();

        services.AddScoped<IAIService, AIService>();
        services.AddScoped<IDeviceIntegrationService, DeviceIntegrationService>();
        services.AddScoped<ITeleSessionService, TeleSessionService>();
        services.Configure<SpeechToTextOptions>(configuration.GetSection(SpeechToTextOptions.SectionName));
        services.AddSingleton<WhisperModelHolder>();
        services.AddSingleton<WhisperSpeechToTextService>();
        services.AddSingleton<AzureSpeechToTextService>();
        services.AddScoped<ISpeechToTextService, SpeechToTextRouter>();

        services.Configure<PatientMentalHealthContentOptions>(configuration.GetSection(PatientMentalHealthContentOptions.SectionName));
        services.AddSingleton<IPatientMentalHealthContentProvider, OptionsPatientMentalHealthContentProvider>();

        services.Configure<PatientSupportOptions>(configuration.GetSection(PatientSupportOptions.SectionName));
        services.AddSingleton<IPatientSupportContentProvider, OptionsPatientSupportContentProvider>();

        services.Configure<PatientProfilePhotoOptions>(configuration.GetSection(PatientProfilePhotoOptions.SectionName));
        services.Configure<AzureStorageOptions>(configuration.GetSection(AzureStorageOptions.SectionName));
        services.Configure<VoiceRecordingStorageOptions>(configuration.GetSection(VoiceRecordingStorageOptions.SectionName));

        var storageConnection = configuration.GetSection(AzureStorageOptions.SectionName)[
            nameof(AzureStorageOptions.ConnectionString)];
        var isDevelopment = environment?.IsDevelopment() == true
            || string.Equals(
                configuration["ASPNETCORE_ENVIRONMENT"],
                Environments.Development,
                StringComparison.OrdinalIgnoreCase);
        if (!string.IsNullOrWhiteSpace(storageConnection))
            services.AddSingleton<IObjectStorage, AzureBlobObjectStorage>();
        else if (isDevelopment)
            services.AddSingleton<IObjectStorage, LocalFileObjectStorage>();
        else
            throw new InvalidOperationException(
                "AzureStorage:ConnectionString is required outside Development. Run scripts/New-RaphCareAzureBlobStorage.ps1.");

        services.AddScoped<IPatientProfilePhotoStorage, PatientProfilePhotoStorage>();
        services.AddScoped<IVoiceRecordingStorage, VoiceRecordingStorage>();

        services.Configure<PatientAssistantAiOptions>(configuration.GetSection(PatientAssistantAiOptions.SectionName));

        services.AddHttpClient(AIService.HttpClientName, client =>
        {
            client.Timeout = TimeSpan.FromSeconds(45);
        });
        services.AddHttpClient();

        services.Configure<FirebasePushOptions>(configuration.GetSection(FirebasePushOptions.SectionName));
        var firebasePush = configuration.GetSection(FirebasePushOptions.SectionName).Get<FirebasePushOptions>();
        if (firebasePush?.IsEnabled == true)
            services.AddScoped<IPatientPushNotificationSender, FirebasePatientPushNotificationSender>();
        else
            services.AddScoped<IPatientPushNotificationSender, NoOpPatientPushNotificationSender>();

        return services;
    }
}
