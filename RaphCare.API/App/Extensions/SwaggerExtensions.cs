using Microsoft.OpenApi.Models;
using RaphCare.API.App.Swagger;

namespace RaphCare.API.App.Extensions;

/// <summary>
/// Centralizes Swagger/OpenAPI registration and pipeline setup for the API.
/// </summary>
public static class SwaggerExtensions
{
    private const string DocName = "v1";
    private const string DocTitle = "RaphCare API";

    /// <summary>
    /// Adds Swagger Gen with Bearer security and optional XML comments.
    /// </summary>
    public static IServiceCollection AddRaphCareSwagger(this IServiceCollection services)
    {
        services.AddSwaggerGen(options =>
        {
            options.SwaggerDoc(DocName, new OpenApiInfo { Title = DocTitle, Version = DocName });
            options.AddSecurityDefinition("Bearer", new OpenApiSecurityScheme
            {
                Name = "Authorization",
                Type = SecuritySchemeType.Http,
                Scheme = "Bearer",
                In = ParameterLocation.Header,
                Description = "JWT Bearer token from Microsoft Entra ID."
            });
            options.AddSecurityRequirement(new OpenApiSecurityRequirement
            {
                {
                    new OpenApiSecurityScheme
                    {
                        Reference = new OpenApiReference { Type = ReferenceType.SecurityScheme, Id = "Bearer" }
                    },
                    Array.Empty<string>()
                }
            });
            var xmlFile = $"{System.Reflection.Assembly.GetExecutingAssembly().GetName().Name}.xml";
            var xmlPath = Path.Combine(AppContext.BaseDirectory, xmlFile);
            if (File.Exists(xmlPath))
                options.IncludeXmlComments(xmlPath);

            options.OperationFilter<StandaloneEmergencyWebhookOperationFilter>();
        });
        return services;
    }

    /// <summary>
    /// Enables Swagger and Swagger UI, and redirects root URL to /swagger.
    /// </summary>
    public static WebApplication UseRaphCareSwagger(this WebApplication app)
    {
        app.UseSwagger();
        app.UseSwaggerUI(options => options.SwaggerEndpoint($"/swagger/{DocName}/swagger.json", $"{DocTitle} {DocName}"));
        app.MapGet("/", () => Results.Redirect("/swagger")).ExcludeFromDescription();
        return app;
    }
}
