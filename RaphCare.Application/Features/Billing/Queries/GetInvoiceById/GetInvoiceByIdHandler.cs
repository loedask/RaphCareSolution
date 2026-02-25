using MediatR;
using RaphCare.Application.Common.Exceptions;
using RaphCare.Application.Common.Interfaces;
using RaphCare.Application.Features.Billing.DTOs;
using RaphCare.Domain.Billing;

namespace RaphCare.Application.Features.Billing.Queries.GetInvoiceById;

public class GetInvoiceByIdHandler : IRequestHandler<GetInvoiceByIdQuery, InvoiceDto>
{
    private readonly IRepository<Invoice> _repository;

    public GetInvoiceByIdHandler(IRepository<Invoice> repository)
    {
        _repository = repository;
    }

    public async Task<InvoiceDto> Handle(GetInvoiceByIdQuery request, CancellationToken cancellationToken)
    {
        var invoice = await _repository.GetByIdAsync(request.Id, cancellationToken);
        if (invoice is null)
        {
            throw new NotFoundException(nameof(Invoice), request.Id);
        }

        return new InvoiceDto
        {
            Id = invoice.Id,
            ClinicId = invoice.ClinicId,
            PatientId = invoice.PatientId,
            VisitId = invoice.VisitId,
            Amount = invoice.Amount,
            Currency = invoice.Currency,
            Status = invoice.Status,
            DueDate = invoice.DueDate,
            PaidAt = invoice.PaidAt
        };
    }
}
