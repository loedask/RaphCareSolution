using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.IdentityModel.Tokens;
using RaphCare.Application.Common.Interfaces;
using RaphCare.Identity.Entra;

namespace RaphCare.Identity;

public static class DependencyInjection
{
    public static IServiceCollection AddIdentity(
        this IServiceCollection services,
        IConfiguration configuration)
    {
        services.Configure<EntraOptions>(configuration.GetSection(EntraOptions.SectionName));

        services.AddScoped<EntraTokenValidator>();
        services.AddScoped<IUserProvisioningService, EntraUserProvisioningService>();
        services.AddScoped<EntraRoleMapper>();

        var entraSection = configuration.GetSection(EntraOptions.SectionName);
        services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
            .AddJwtBearer(options =>
            {
                var authority = entraSection["Authority"]?.TrimEnd('/')
                    ?? $"https://login.microsoftonline.com/{entraSection["TenantId"]}/v2.0";
                var audience = entraSection["Audience"];
                var validIssuers = entraSection.GetSection("ValidIssuers").Get<string[]>();
                if (validIssuers == null || validIssuers.Length == 0)
                    validIssuers = new[] { $"{authority}/", authority };

                options.TokenValidationParameters = new TokenValidationParameters
                {
                    ValidAudience = audience,
                    ValidIssuers = validIssuers,
                    ValidateIssuer = true,
                    ValidateAudience = !string.IsNullOrWhiteSpace(audience),
                    ValidateLifetime = true,
                    ValidateIssuerSigningKey = true,
                    ClockSkew = TimeSpan.FromMinutes(2)
                };

                options.Events = new JwtBearerEvents
                {
                    OnTokenValidated = async context =>
                    {
                        if (context.Principal == null)
                            return;
                        var provisioning = context.HttpContext.RequestServices
                            .GetRequiredService<IUserProvisioningService>();
                        await provisioning.EnsureUserExistsAsync(context.Principal, context.HttpContext.RequestAborted);
                    }
                };
            });

        return services;
    }
}
