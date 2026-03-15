using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using RaphCare.Application.Features.Billing.Commands.CreateInvoice;
using RaphCare.Application.Features.Billing.Commands.UpdateInvoice;
using RaphCare.Application.Features.Billing.Queries.GetInvoiceById;
using RaphCare.Application.Features.Billing.Queries.GetInvoices;

namespace RaphCare.API.Controllers;

/// <summary>Invoicing and billing. Thin API; delegates to MediatR. Roles: Admin, Provider.</summary>
[Authorize(Policy = "RequireAdmin")]
[ApiController]
[Route("api/[controller]")]
public class BillingController(IMediator mediator) : ControllerBase
{
    /// <summary>Get an invoice by id.</summary>
    [HttpGet("{id:guid}")]
    public async Task<IActionResult> Get(Guid id, CancellationToken cancellationToken)
    {
        var result = await mediator.Send(new GetInvoiceByIdQuery { Id = id }, cancellationToken);
        return Ok(result);
    }

    /// <summary>Get paginated list of invoices.</summary>
    [HttpGet]
    public async Task<IActionResult> GetList([FromQuery] int pageNumber = 1, [FromQuery] int pageSize = 20, CancellationToken cancellationToken = default)
    {
        var result = await mediator.Send(new GetInvoicesQuery { PageNumber = pageNumber, PageSize = pageSize }, cancellationToken);
        return Ok(result);
    }

    /// <summary>Create a new invoice.</summary>
    [HttpPost]
    public async Task<IActionResult> Create([FromBody] CreateInvoiceCommand command, CancellationToken cancellationToken)
    {
        var id = await mediator.Send(command, cancellationToken);
        return CreatedAtAction(nameof(Get), new { id }, new { id });
    }

    /// <summary>Update an existing invoice.</summary>
    [HttpPut("{id:guid}")]
    public async Task<IActionResult> Update(Guid id, [FromBody] UpdateInvoiceCommand command, CancellationToken cancellationToken)
    {
        if (id != command.Id) return BadRequest();
        await mediator.Send(command, cancellationToken);
        return NoContent();
    }
}
