using MediatR;

namespace RaphCare.Application.Features.Billing.Commands.UpdateInvoice;

public class UpdateInvoiceCommand : IRequest<Unit>
{
    public Guid Id { get; set; }
    public string? Status { get; set; }
    public DateTime? PaidAt { get; set; }
}
