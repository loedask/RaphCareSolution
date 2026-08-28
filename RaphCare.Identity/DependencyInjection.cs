using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.IdentityModel.Protocols;
using Microsoft.IdentityModel.Protocols.OpenIdConnect;
using Microsoft.IdentityModel.Tokens;
using RaphCare.Application.Common.Interfaces;
using RaphCare.Identity.Entra;
using RaphCare.Identity.LocalJwt;

namespace RaphCare.Identity;

public static class DependencyInjection
{
    public static IServiceCollection AddIdentity(
        this IServiceCollection services,
        IConfiguration configuration)
    {
        services.Configure<EntraOptions>(configuration.GetSection(EntraOptions.SectionName));
        services.Configure<LocalJwtOptions>(configuration.GetSection(LocalJwtOptions.SectionName));

        services.AddScoped<EntraTokenValidator>();
        services.AddScoped<IUserProvisioningService, EntraUserProvisioningService>();
        services.AddScoped<EntraRoleMapper>();

        var entraSection = configuration.GetSection(EntraOptions.SectionName);
        var localJwt = configuration.GetSection(LocalJwtOptions.SectionName).Get<LocalJwtOptions>() ?? new LocalJwtOptions();

        var entraAuthority = entraSection["Authority"]?.TrimEnd('/')
            ?? $"https://login.microsoftonline.com/{entraSection["TenantId"]}/v2.0";
        var entraAudience = entraSection["Audience"];
        var entraIssuers = entraSection.GetSection("ValidIssuers").Get<string[]>();
        if (entraIssuers == null || entraIssuers.Length == 0)
            entraIssuers = [$"{entraAuthority}/", entraAuthority];

        var validIssuers = new List<string>(entraIssuers);
        if (!string.IsNullOrWhiteSpace(localJwt.Issuer) && !validIssuers.Contains(localJwt.Issuer, StringComparer.Ordinal))
            validIssuers.Add(localJwt.Issuer);

        var validAudiences = new List<string>();
        if (!string.IsNullOrWhiteSpace(entraAudience))
            validAudiences.Add(entraAudience);
        if (!string.IsNullOrWhiteSpace(localJwt.Audience))
            validAudiences.Add(localJwt.Audience);
        validAudiences.Add("RaphCare.Web");

        services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
            .AddJwtBearer(options =>
            {
                options.Authority = entraAuthority;
                options.Audience = entraAudience;

                options.MapInboundClaims = false;

                options.TokenValidationParameters = new TokenValidationParameters
                {
                    ValidIssuers = validIssuers,
                    ValidAudiences = validAudiences.Count > 0 ? validAudiences : null,
                    ValidateIssuer = validIssuers.Count > 0,
                    ValidateAudience = validAudiences.Count > 0,
                    ValidateLifetime = true,
                    ValidateIssuerSigningKey = true,
                    ClockSkew = TimeSpan.FromMinutes(2),
                    NameClaimType = JwtRegisteredClaimNames.Sub,
                    RoleClaimType = ClaimTypes.Role,
                    IssuerSigningKeyResolver = (token, securityToken, kid, parameters) =>
                        ResolveSigningKeys(token, securityToken, entraAuthority, localJwt)
                };

                options.Events = new JwtBearerEvents
                {
                    OnTokenValidated = async context =>
                    {
                        if (context.Principal is null)
                            return;

                        var rawToken = context.Request.Headers.Authorization.ToString();
                        if (LocalJwtTokenHelper.IsLocalEmailJwt(rawToken, context.SecurityToken, localJwt)
                            || LocalJwtTokenHelper.IsLocalEmailPrincipal(context.Principal, localJwt))
                        {
                            LocalJwtTokenHelper.ApplyRoleClaimsForAuthorization(context.Principal);
                            return;
                        }

                        if (!LocalJwtTokenHelper.HasEntraObjectId(context.Principal))
                            return;

                        var provisioning = context.HttpContext.RequestServices
                            .GetRequiredService<IUserProvisioningService>();
                        await provisioning.EnsureUserExistsAsync(context.Principal, context.HttpContext.RequestAborted);
                        EntraRoleMapper.ApplyAuthorizationRoleClaims(context.Principal);
                    }
                };
            });

        return services;
    }

    private static List<SecurityKey> ResolveSigningKeys(
        string rawToken,
        SecurityToken securityToken,
        string entraAuthority,
        LocalJwtOptions localJwt)
    {
        var keys = new List<SecurityKey>();

        if (!string.IsNullOrWhiteSpace(localJwt.Secret))
            keys.Add(LocalJwtSigningKeyHelper.CreateSigningKey(localJwt.Secret));

        if (LocalJwtTokenHelper.IsLocalEmailJwt(rawToken, securityToken, localJwt))
            return keys;

        try
        {
            keys.AddRange(GetEntraSigningKeys(entraAuthority));
        }
        catch
        {
            // Entra metadata unavailable (offline dev); local key above may still validate email JWTs.
        }

        return keys;
    }

    private static IEnumerable<SecurityKey> GetEntraSigningKeys(string entraAuthority)
    {
        var metadataAddress = $"{entraAuthority}/.well-known/openid-configuration";
        var configManager = new ConfigurationManager<OpenIdConnectConfiguration>(
            metadataAddress,
            new OpenIdConnectConfigurationRetriever());
        var config = configManager.GetConfigurationAsync(CancellationToken.None).GetAwaiter().GetResult();
        return config.SigningKeys;
    }
}
