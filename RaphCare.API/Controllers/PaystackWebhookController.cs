using System.Text.Json;
using System.Text.Json.Serialization;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using RaphCare.Application.Features.PatientBilling.Commands.CompleteCarePlanCheckoutFromPaystack;

namespace RaphCare.API.Controllers;

/// <summary>Paystack charge webhooks. Configure the URL in the Paystack dashboard.</summary>
[AllowAnonymous]
[ApiController]
[Route("api/webhooks/paystack")]
public sealed class PaystackWebhookController(IMediator mediator) : ControllerBase
{
    [HttpPost(Name = "PaystackWebhook")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    public async Task<IActionResult> Post(CancellationToken cancellationToken)
    {
        using var doc = await JsonDocument.ParseAsync(Request.Body, cancellationToken: cancellationToken)
            .ConfigureAwait(false);
        var root = doc.RootElement;
        var eventName = root.TryGetProperty("event", out var ev) ? ev.GetString() : null;
        if (!string.Equals(eventName, "charge.success", StringComparison.OrdinalIgnoreCase))
            return Ok();

        string? reference = null;
        if (root.TryGetProperty("data", out var data) && data.TryGetProperty("reference", out var pref))
            reference = pref.GetString();

        if (!string.IsNullOrWhiteSpace(reference))
        {
            await mediator.Send(
                    new CompleteCarePlanCheckoutFromPaystackCommand { Reference = reference },
                    cancellationToken)
                .ConfigureAwait(false);
        }

        return Ok();
    }
}
