using RaphCare.API.App.Extensions;
using RaphCare.API.Services;
using RaphCare.API.App.Services;
using RaphCare.API.App.Middleware;
using RaphCare.Application.Common.Interfaces;
using RaphCare.Application;
using RaphCare.Identity;
using RaphCare.Infrastructure;
using RaphCare.Persistence;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddApplication();
builder.Services.AddInfrastructure(builder.Configuration);
builder.Services.AddPersistence(builder.Configuration);
builder.Services.AddIdentity(builder.Configuration);
builder.Services.AddHttpContextAccessor();
builder.Services.AddScoped<IClinicContext, ClinicContext>();

builder.Services.AddControllers();

builder.Services.AddScoped<IFhirExportAuditLogger, FhirExportAuditLogger>();

builder.Services.AddAuthorization(options =>
{
    options.AddPolicy("RequireAdmin", policy => policy.RequireRole("Administrator"));
    options.AddPolicy("RequireProvider", policy => policy.RequireRole("Administrator", "Clinician"));
    options.AddPolicy("RequirePatient", policy => policy.RequireRole("Patient", "Administrator", "Clinician"));
});

builder.Services.AddRaphCareSwagger();

var app = builder.Build();

if (app.Environment.IsDevelopment())
{
    app.UseRaphCareSwagger();
    await app.ApplyMigrationsAsync();
}

app.UseMiddleware<ExceptionHandlingMiddleware>();
// Development: mobile emulator calls http://10.0.2.2:5281; HTTPS redirect breaks on untrusted dev certs.
if (!app.Environment.IsDevelopment())
    app.UseHttpsRedirection();
app.UseAuthentication();
app.UseAuthorization();
app.UseMiddleware<TenantResolutionMiddleware>();
app.UseMiddleware<AuditMiddleware>();

app.MapControllers();

app.Run();
