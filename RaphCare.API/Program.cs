using RaphCare.API.App.Extensions;
using RaphCare.API.Services;
using RaphCare.API.App.Services;
using RaphCare.API.App.Middleware;
using RaphCare.Application.Common.Interfaces;
using RaphCare.Application;
using RaphCare.Identity;
using RaphCare.Infrastructure;
using RaphCare.Persistence;

using RaphCare.Domain.Identity;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddApplication();
builder.Services.AddInfrastructure(builder.Configuration, builder.Environment);
builder.Services.AddPersistence(builder.Configuration);
builder.Services.AddIdentity(builder.Configuration);
builder.Services.AddHttpContextAccessor();
builder.Services.AddScoped<IClinicContext, ClinicContext>();

builder.Services.AddControllers();

builder.Services.AddScoped<IFhirExportAuditLogger, FhirExportAuditLogger>();

builder.Services.AddAuthorization(options =>
{
    options.AddPolicy("RequireAdmin", policy => policy.RequireRole(RaphCareRoles.Administrator));
    options.AddPolicy("RequireProvider", policy => policy.RequireRole(RaphCareRoles.Provider));
    options.AddPolicy("RequirePatient", policy => policy.RequireRole(RaphCareRoles.PatientPortal));
    options.AddPolicy("RequirePlatformAdmin", policy =>
    {
        policy.RequireAssertion(context =>
        {
            if (context.Resource is not HttpContext httpContext)
                return false;

            var config = httpContext.RequestServices.GetRequiredService<IConfiguration>();
            var env = httpContext.RequestServices.GetRequiredService<IHostEnvironment>();
            if (env.IsDevelopment() && config.GetValue("Admin:AllowAnonymousInDevelopment", true))
                return true;

            return context.User.IsInRole(RaphCareRoles.Administrator);
        });
    });
});

builder.Services.AddCors(options =>
{
    options.AddPolicy("RaphCareWebAdmin", policy =>
    {
        var origins = new HashSet<string>(StringComparer.OrdinalIgnoreCase)
        {
            "http://localhost:5177",
            "https://localhost:7092"
        };

        foreach (var origin in builder.Configuration.GetSection("Cors:WebAdminOrigins").Get<string[]>() ?? [])
        {
            if (!string.IsNullOrWhiteSpace(origin))
                origins.Add(origin.Trim().TrimEnd('/'));
        }

        var portalBaseUrl = builder.Configuration["RaphCare:WebPortalBaseUrl"];
        if (!string.IsNullOrWhiteSpace(portalBaseUrl))
            origins.Add(portalBaseUrl.Trim().TrimEnd('/'));

        policy.WithOrigins(origins.ToArray())
            .AllowAnyHeader()
            .AllowAnyMethod();
    });
});

builder.Services.AddRaphCareSwagger();

var app = builder.Build();

if (app.Environment.IsDevelopment())
    app.UseRaphCareSwagger();

if (app.Environment.IsDevelopment() || app.Environment.IsStaging())
    await app.ApplyMigrationsAsync();

app.UseMiddleware<ExceptionHandlingMiddleware>();
// Development: mobile emulator calls http://10.0.2.2:5281; HTTPS redirect breaks on untrusted dev certs.
if (!app.Environment.IsDevelopment())
    app.UseHttpsRedirection();
app.UseCors("RaphCareWebAdmin");
app.UseAuthentication();
app.UseAuthorization();
app.UseMiddleware<TenantResolutionMiddleware>();
app.UseMiddleware<AuditMiddleware>();

app.MapControllers();

app.Run();
