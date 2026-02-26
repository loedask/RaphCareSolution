using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using RaphCare.Application.Common.Interfaces;
using RaphCare.Infrastructure.Persistence;
using RaphCare.Infrastructure.Persistence.Interceptors;
using RaphCare.Infrastructure.Services;

namespace RaphCare.Infrastructure;

public static class DependencyInjection
{
    public static IServiceCollection AddInfrastructure(
        this IServiceCollection services,
        IConfiguration configuration)
    {
        services.AddScoped<IDomainEventDispatcher, DomainEventDispatcher>();
        services.AddScoped<AuditableEntityInterceptor>();
        services.AddScoped<SoftDeleteInterceptor>();
        services.AddScoped<DomainEventDispatcherInterceptor>();

        services.AddScoped<IEmailService, EmailService>();
        services.AddScoped<ISmsService, SmsService>();
        services.AddScoped<IPaymentGatewayService, PaymentGatewayService>();
        services.AddScoped<IAIService, AIService>();
        services.AddScoped<IDeviceIntegrationService, DeviceIntegrationService>();
        services.AddScoped<ITeleSessionService, TeleSessionService>();

        return services;
    }
}
