using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using RaphCare.Application.Common.Configuration;
using RaphCare.Application.Common.Interfaces;
using RaphCare.Infrastructure.Configuration;
using RaphCare.Infrastructure.Persistence;
using RaphCare.Infrastructure.Persistence.Interceptors;
using RaphCare.Infrastructure.Services;
using RaphCare.Infrastructure.StandaloneEmergency;
using RaphCare.Infrastructure.Telehealth;

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
        IConfiguration configuration)
    {
        services.AddScoped<IDomainEventDispatcher, DomainEventDispatcher>();
        services.AddScoped<AuditableEntityInterceptor>();
        services.AddScoped<SoftDeleteInterceptor>();
        services.AddScoped<DomainEventDispatcherInterceptor>();

        services.AddScoped<ICurrentUserService, CurrentUserService>();
        services.AddScoped<IDateTimeProvider, DateTimeProvider>();

        services.AddScoped<IEmailService, EmailService>();

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
        services.AddScoped<IPaymentGatewayService, PaymentGatewayService>();
        services.AddScoped<IAIService, AIService>();
        services.AddScoped<IDeviceIntegrationService, DeviceIntegrationService>();
        services.AddScoped<ITeleSessionService, TeleSessionService>();
        services.AddScoped<ISpeechToTextService, AzureSpeechToTextService>();

        services.Configure<PatientMentalHealthContentOptions>(configuration.GetSection(PatientMentalHealthContentOptions.SectionName));
        services.AddSingleton<IPatientMentalHealthContentProvider, OptionsPatientMentalHealthContentProvider>();

        return services;
    }
}
