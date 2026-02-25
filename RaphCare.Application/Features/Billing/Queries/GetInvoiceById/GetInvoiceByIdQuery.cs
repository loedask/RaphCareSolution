using MediatR;
using RaphCare.Application.Features.Billing.DTOs;

namespace RaphCare.Application.Features.Billing.Queries.GetInvoiceById;

public class GetInvoiceByIdQuery : IRequest<InvoiceDto>
{
    public Guid Id { get; set; }
}
