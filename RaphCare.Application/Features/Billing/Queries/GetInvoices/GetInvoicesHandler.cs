using MediatR;
using RaphCare.Application.Common.DTOs;
using RaphCare.Application.Common.Interfaces;
using RaphCare.Application.Features.Billing.DTOs;
using RaphCare.Domain.Billing;

namespace RaphCare.Application.Features.Billing.Queries.GetInvoices;

public class GetInvoicesHandler : IRequestHandler<GetInvoicesQuery, PagedResult<InvoiceDto>>
{
    private readonly IRepository<Invoice> _repository;

    public GetInvoicesHandler(IRepository<Invoice> repository)
    {
        _repository = repository;
    }

    public async Task<PagedResult<InvoiceDto>> Handle(GetInvoicesQuery request, CancellationToken cancellationToken)
    {
        var invoices = await _repository.ListAsync(cancellationToken);

        var totalCount = invoices.Count;

        var items = invoices
            .Skip((request.PageNumber - 1) * request.PageSize)
            .Take(request.PageSize)
            .Select(i => new InvoiceDto
            {
                Id = i.Id,
                ClinicId = i.ClinicId,
                PatientId = i.PatientId,
                VisitId = i.VisitId,
                Amount = i.Amount,
                Currency = i.Currency,
                Status = i.Status,
                DueDate = i.DueDate,
                PaidAt = i.PaidAt
            })
            .ToList();

        return new PagedResult<InvoiceDto>
        {
            Items = items,
            TotalCount = totalCount,
            PageNumber = request.PageNumber,
            PageSize = request.PageSize
        };
    }
}
