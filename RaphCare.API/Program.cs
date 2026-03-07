using Microsoft.AspNetCore.Authentication.JwtBearer;
using RaphCare.API.App.Extensions;
using RaphCare.API.Middleware;
using RaphCare.Application;
using RaphCare.Identity;
using RaphCare.Infrastructure;
using RaphCare.Persistence;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddApplication();
builder.Services.AddInfrastructure(builder.Configuration);
builder.Services.AddPersistence(builder.Configuration);
builder.Services.AddIdentity(builder.Configuration);

builder.Services.AddControllers();
builder.Services.AddHttpContextAccessor();

builder.Services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme);
builder.Services.AddAuthorization(options =>
{
    options.AddPolicy("RequireAdmin", policy => policy.RequireRole("Administrator"));
    options.AddPolicy("RequireProvider", policy => policy.RequireRole("Administrator", "Clinician"));
    options.AddPolicy("RequirePatient", policy => policy.RequireRole("Patient", "Administrator", "Clinician"));
});

builder.Services.AddRaphCareSwagger();

var app = builder.Build();

app.UseMiddleware<ExceptionHandlingMiddleware>();
app.UseHttpsRedirection();
app.UseAuthentication();
app.UseAuthorization();
app.UseMiddleware<TenantResolutionMiddleware>();
app.UseMiddleware<AuditMiddleware>();

app.MapControllers();

if (app.Environment.IsDevelopment())
{

    app.UseRaphCareSwagger();

    await app.ApplyMigrationsAsync();
}

app.Run();
