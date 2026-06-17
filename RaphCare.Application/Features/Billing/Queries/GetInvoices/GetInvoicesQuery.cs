using MediatR;
using RaphCare.Application.Common.DTOs;
using RaphCare.Application.Features.Billing.DTOs;

namespace RaphCare.Application.Features.Billing.Queries.GetInvoices;

public class GetInvoicesQuery : IRequest<PagedResult<InvoiceDto>>
{
    public int PageNumber { get; set; } = 1;
    public int PageSize { get; set; } = 20;
}
