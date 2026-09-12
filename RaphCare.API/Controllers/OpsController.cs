using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using RaphCare.Application.Features.Billing.Commands.UpdatePriceCatalogItem;
using RaphCare.Application.Features.Billing.DTOs;
using RaphCare.Application.Features.Billing.Queries.GetPriceCatalog;
using RaphCare.Application.Features.Ops.Commands.UpdateClinicCommercialPlan;
using RaphCare.Application.Features.Ops.Queries.GetClinicCommercialPlan;
using RaphCare.Application.Features.Ops.Queries.GetPlatformOpsStats;

namespace RaphCare.API.Controllers;

/// <summary>Platform Ops dashboard endpoints (RequirePlatformAdmin).</summary>
[ApiController]
[Route("api/ops")]
[Authorize(Policy = "RequirePlatformAdmin")]
public sealed class OpsController(IMediator mediator) : ControllerBase
{
    /// <summary>Platform-wide counts for the Ops home screen.</summary>
    [HttpGet("stats", Name = "GetPlatformOpsStats")]
    [ProducesResponseType(typeof(PlatformOpsStatsDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    public async Task<IActionResult> GetStats(CancellationToken cancellationToken)
    {
        var result = await mediator.Send(new GetPlatformOpsStatsQuery(), cancellationToken)
            .ConfigureAwait(false);
        if (result is null)
            return Forbid();
        return Ok(result);
    }

    /// <summary>Commercial price catalog (list ZAR and USD amounts).</summary>
    [HttpGet("price-catalog", Name = "GetPriceCatalog")]
    [ProducesResponseType(typeof(IReadOnlyList<PriceCatalogItemDto>), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    public async Task<IActionResult> GetPriceCatalog(
        [FromQuery] bool activeOnly = true,
        CancellationToken cancellationToken = default)
    {
        var result = await mediator.Send(
                new GetPriceCatalogQuery { ActiveOnly = activeOnly },
                cancellationToken)
            .ConfigureAwait(false);
        return Ok(result);
    }

    /// <summary>Update ZAR and USD list amounts for one catalog SKU.</summary>
    [HttpPut("price-catalog/{id:guid}", Name = "UpdatePriceCatalogItem")]
    [ProducesResponseType(typeof(PriceCatalogItemDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    public async Task<IActionResult> UpdatePriceCatalogItem(
        Guid id,
        [FromBody] UpdatePriceCatalogItemCommand body,
        CancellationToken cancellationToken)
    {
        var result = await mediator.Send(
                new UpdatePriceCatalogItemCommand
                {
                    Id = id,
                    AmountZar = body.AmountZar,
                    AmountUsd = body.AmountUsd
                },
                cancellationToken)
            .ConfigureAwait(false);
        return result is null ? NotFound() : Ok(result);
    }

    /// <summary>Commercial site plan for a hospital.</summary>
    [HttpGet("clinics/{clinicId:guid}/commercial-plan", Name = "GetClinicCommercialPlan")]
    [ProducesResponseType(typeof(ClinicCommercialPlanDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    public async Task<IActionResult> GetClinicCommercialPlan(
        Guid clinicId,
        CancellationToken cancellationToken)
    {
        var result = await mediator.Send(
                new GetClinicCommercialPlanQuery { ClinicId = clinicId },
                cancellationToken)
            .ConfigureAwait(false);
        return result is null ? NotFound() : Ok(result);
    }

    /// <summary>Assign Practice, Clinic, Hospital, or Network to a hospital.</summary>
    [HttpPut("clinics/{clinicId:guid}/commercial-plan", Name = "UpdateClinicCommercialPlan")]
    [ProducesResponseType(typeof(ClinicCommercialPlanDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    public async Task<IActionResult> UpdateClinicCommercialPlan(
        Guid clinicId,
        [FromBody] UpdateClinicCommercialPlanCommand body,
        CancellationToken cancellationToken)
    {
        var result = await mediator.Send(
                new UpdateClinicCommercialPlanCommand
                {
                    ClinicId = clinicId,
                    CommercialPlan = body.CommercialPlan
                },
                cancellationToken)
            .ConfigureAwait(false);
        return result is null ? NotFound() : Ok(result);
    }
}
