using System.Text;
using System.Text.Json;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Net.Mime;
using RaphCare.Application.Common.Interfaces;
using RaphCare.Application.Features.StandaloneEmergency.Commands.IngestDeviceEmergencyEvent;
using RaphCare.Application.Features.StandaloneEmergency.DTOs;

namespace RaphCare.API.Controllers;

/// <summary>OEM / carrier webhook for 4G standalone emergency devices (Y6-class). HMAC optional in Development when configured.</summary>
[ApiController]
[AllowAnonymous]
[Route("api/integrations/standalone-emergency")]
public sealed class StandaloneEmergencyWebhookController(
    IMediator mediator,
    IStandaloneEmergencyWebhookSignatureValidator signatureValidator) : ControllerBase
{
    private static readonly JsonSerializerOptions JsonOptions = new()
    {
        PropertyNameCaseInsensitive = true
    };

    private readonly IMediator _mediator = mediator;
    private readonly IStandaloneEmergencyWebhookSignatureValidator _signatureValidator = signatureValidator;

    /// <summary>Accepts JSON body; validates <c>X-RaphCare-Emergency-Signature</c> (hex HMAC-SHA256 of raw UTF-8 bytes) when a shared secret is configured.</summary>
    /// <remarks>OpenAPI request schema: <see cref="IngestDeviceEmergencyEventCommand"/> (see also <c>StandaloneEmergencyWebhookOperationFilter</c>).</remarks>
    [HttpPost("events")]
    [Consumes(MediaTypeNames.Application.Json)]
    [ProducesResponseType(typeof(IngestDeviceEmergencyEventResult), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    public async Task<IActionResult> Ingest(CancellationToken cancellationToken)
    {
        Request.EnableBuffering();
        string rawBody;
        using (var reader = new StreamReader(Request.Body, Encoding.UTF8, detectEncodingFromByteOrderMarks: false, bufferSize: 1024, leaveOpen: true))
        {
            rawBody = await reader.ReadToEndAsync(cancellationToken).ConfigureAwait(false);
        }

        Request.Body.Position = 0;

        var bodyUtf8 = Encoding.UTF8.GetBytes(rawBody);
        var sig = Request.Headers["X-RaphCare-Emergency-Signature"].FirstOrDefault();
        if (!_signatureValidator.IsValid(sig, bodyUtf8))
            return Unauthorized();

        IngestDeviceEmergencyEventCommand? command;
        try
        {
            command = JsonSerializer.Deserialize<IngestDeviceEmergencyEventCommand>(rawBody, JsonOptions);
        }
        catch (JsonException)
        {
            return BadRequest("Invalid JSON.");
        }

        if (command is null)
            return BadRequest();

        var result = await _mediator.Send(command, cancellationToken).ConfigureAwait(false);
        return Ok(result);
    }
}
