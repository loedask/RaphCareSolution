using Microsoft.OpenApi.Models;
using RaphCare.API.Controllers;
using RaphCare.Application.Features.StandaloneEmergency.Commands.IngestDeviceEmergencyEvent;
using Swashbuckle.AspNetCore.SwaggerGen;

namespace RaphCare.API.App.Swagger;

/// <summary>Documents JSON body and HMAC header for <see cref="StandaloneEmergencyWebhookController.Ingest"/> (raw body is read in the action for signature verification).</summary>
public sealed class StandaloneEmergencyWebhookOperationFilter : IOperationFilter
{
    public void Apply(OpenApiOperation operation, OperationFilterContext context)
    {
        if (context.MethodInfo?.DeclaringType != typeof(StandaloneEmergencyWebhookController))
            return;

        if (context.MethodInfo.Name != nameof(StandaloneEmergencyWebhookController.Ingest))
            return;

        operation.RequestBody = new OpenApiRequestBody
        {
            Required = true,
            Description = "JSON payload. HMAC must be computed over the exact UTF-8 bytes sent as this body.",
            Content =
            {
                ["application/json"] = new OpenApiMediaType
                {
                    Schema = context.SchemaGenerator.GenerateSchema(
                        typeof(IngestDeviceEmergencyEventCommand),
                        context.SchemaRepository)
                }
            }
        };

        operation.Parameters ??= new List<OpenApiParameter>();
        operation.Parameters.Add(new OpenApiParameter
        {
            Name = "X-RaphCare-Emergency-Signature",
            In = ParameterLocation.Header,
            Required = false,
            Description = "Hex-encoded HMAC-SHA256 of the raw UTF-8 request body. Optional prefix `sha256=` is accepted. Required when StandaloneEmergency:WebhookSharedSecret is set (unless Development unsigned mode).",
            Schema = new OpenApiSchema { Type = "string" }
        });
    }
}
