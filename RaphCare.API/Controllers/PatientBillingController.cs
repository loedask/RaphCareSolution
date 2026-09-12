using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using RaphCare.API.App.Contracts;
using RaphCare.Application.Common.DTOs;
using RaphCare.Application.Features.PatientBilling.Commands.AddMyPaymentMethod;
using RaphCare.Application.Features.PatientBilling.Commands.RemoveMyPaymentMethod;
using RaphCare.Application.Features.PatientBilling.Commands.SetDefaultMyPaymentMethod;
using RaphCare.Application.Features.PatientBilling.Commands.ConfirmCarePlanCheckout;
using RaphCare.Application.Features.PatientBilling.Commands.InitializeCarePlanCheckout;
using RaphCare.Application.Features.PatientBilling.Commands.UpgradeMyBillingPlan;
using RaphCare.Application.Features.PatientBilling.DTOs;
using RaphCare.Application.Features.PatientBilling.Queries.GetMyPatientCarePlan;
using RaphCare.Application.Features.PatientBilling.Queries.GetMyPatientInvoices;
using RaphCare.Application.Features.PatientBilling.Queries.GetMyPaymentMethods;
using RaphCare.Application.Features.PatientBilling.Queries.GetPatientBillingPlanOptions;

namespace RaphCare.API.Controllers;

/// <summary>Patient billing: plans, payment methods, invoice history (JWT with <c>patientId</c>).</summary>
[Authorize(Policy = "RequirePatient")]
[ApiController]
[Route("api/patient/billing")]
public sealed class PatientBillingController(IMediator mediator) : ControllerBase
{
    private readonly IMediator _mediator = mediator;

    [HttpGet("plans", Name = "GetPatientBillingPlanOptions")]
    [ProducesResponseType(typeof(IReadOnlyList<PatientBillingPlanOptionDto>), StatusCodes.Status200OK)]
    public async Task<IActionResult> GetPlans(CancellationToken cancellationToken)
    {
        var result = await _mediator.Send(new GetPatientBillingPlanOptionsQuery(), cancellationToken).ConfigureAwait(false);
        return Ok(result);
    }

    [HttpGet("care-plan", Name = "GetMyPatientCarePlan")]
    [ProducesResponseType(typeof(PatientCarePlanDto), StatusCodes.Status200OK)]
    public async Task<IActionResult> GetCarePlan(CancellationToken cancellationToken)
    {
        var result = await _mediator.Send(new GetMyPatientCarePlanQuery(), cancellationToken).ConfigureAwait(false);
        return Ok(result);
    }

    [HttpGet("invoices", Name = "GetMyPatientInvoices")]
    [ProducesResponseType(typeof(PagedResult<PatientInvoiceHistoryItemDto>), StatusCodes.Status200OK)]
    public async Task<IActionResult> GetInvoices([FromQuery] int pageNumber = 1, [FromQuery] int pageSize = 20, CancellationToken cancellationToken = default)
    {
        var result = await _mediator.Send(
            new GetMyPatientInvoicesQuery { PageNumber = pageNumber, PageSize = pageSize },
            cancellationToken).ConfigureAwait(false);
        return Ok(result);
    }

    [HttpGet("payment-methods", Name = "GetMyPaymentMethods")]
    [ProducesResponseType(typeof(IReadOnlyList<PatientPaymentMethodDto>), StatusCodes.Status200OK)]
    public async Task<IActionResult> GetPaymentMethods(CancellationToken cancellationToken)
    {
        var result = await _mediator.Send(new GetMyPaymentMethodsQuery(), cancellationToken).ConfigureAwait(false);
        return Ok(result);
    }

    [HttpPost("payment-methods", Name = "AddMyPaymentMethod")]
    [ProducesResponseType(typeof(CreatedGuidResponse), StatusCodes.Status201Created)]
    public async Task<IActionResult> AddPaymentMethod([FromBody] AddMyPaymentMethodCommand command, CancellationToken cancellationToken)
    {
        var id = await _mediator.Send(command, cancellationToken).ConfigureAwait(false);
        return CreatedAtRoute("GetMyPaymentMethods", null, new CreatedGuidResponse { Id = id });
    }

    [HttpPut("payment-methods/{id:guid}/default", Name = "SetDefaultMyPaymentMethod")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    public async Task<IActionResult> SetDefaultPaymentMethod(Guid id, CancellationToken cancellationToken)
    {
        await _mediator.Send(new SetDefaultMyPaymentMethodCommand { PaymentMethodId = id }, cancellationToken).ConfigureAwait(false);
        return NoContent();
    }

    [HttpDelete("payment-methods/{id:guid}", Name = "RemoveMyPaymentMethod")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    public async Task<IActionResult> RemovePaymentMethod(Guid id, CancellationToken cancellationToken)
    {
        await _mediator.Send(new RemoveMyPaymentMethodCommand { PaymentMethodId = id }, cancellationToken).ConfigureAwait(false);
        return NoContent();
    }

    [HttpPost("upgrade", Name = "UpgradeMyBillingPlan")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    public async Task<IActionResult> Upgrade([FromBody] UpgradeMyBillingPlanCommand command, CancellationToken cancellationToken)
    {
        await _mediator.Send(command, cancellationToken).ConfigureAwait(false);
        return NoContent();
    }

    [HttpPost("checkout/initialize", Name = "InitializeCarePlanCheckout")]
    [ProducesResponseType(typeof(CarePlanCheckoutDto), StatusCodes.Status200OK)]
    public async Task<IActionResult> InitializeCheckout(
        [FromBody] InitializeCarePlanCheckoutCommand command,
        CancellationToken cancellationToken)
    {
        var result = await _mediator.Send(command, cancellationToken).ConfigureAwait(false);
        return Ok(result);
    }

    [HttpPost("checkout/confirm", Name = "ConfirmCarePlanCheckout")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    public async Task<IActionResult> ConfirmCheckout(
        [FromBody] ConfirmCarePlanCheckoutCommand command,
        CancellationToken cancellationToken)
    {
        await _mediator.Send(command, cancellationToken).ConfigureAwait(false);
        return NoContent();
    }
}
