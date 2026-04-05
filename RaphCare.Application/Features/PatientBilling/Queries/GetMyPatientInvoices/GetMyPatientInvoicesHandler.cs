using MediatR;
using RaphCare.Application.Common.DTOs;
using RaphCare.Application.Common.Exceptions;
using RaphCare.Application.Common.Interfaces;
using RaphCare.Application.Features.PatientBilling.DTOs;
using RaphCare.Domain.Billing;

namespace RaphCare.Application.Features.PatientBilling.Queries.GetMyPatientInvoices;

public sealed class GetMyPatientInvoicesHandler(
    IRepository<Invoice> invoices,
    ICurrentUserService currentUser) : IRequestHandler<GetMyPatientInvoicesQuery, PagedResult<PatientInvoiceHistoryItemDto>>
{
    private readonly IRepository<Invoice> _invoices = invoices;
    private readonly ICurrentUserService _currentUser = currentUser;

    public async Task<PagedResult<PatientInvoiceHistoryItemDto>> Handle(
        GetMyPatientInvoicesQuery request,
        CancellationToken cancellationToken)
    {
        var patientId = _currentUser.CurrentPatientId
            ?? throw new ForbiddenAccessException("A patient profile is required.");

        var pageNumber = request.PageNumber < 1 ? 1 : request.PageNumber;
        var pageSize = request.PageSize < 1 ? 20 : Math.Min(request.PageSize, 100);

        var paged = await _invoices.SearchAsync(
            q => q.Where(i => i.PatientId == patientId).OrderByDescending(i => i.DueDate),
            pageNumber,
            pageSize,
            applyDefaultIdOrdering: false,
            cancellationToken).ConfigureAwait(false);

        var items = paged.Items.Select(i => new PatientInvoiceHistoryItemDto
        {
            Id = i.Id,
            Amount = i.Amount,
            Currency = i.Currency,
            Status = i.Status,
            DueDate = i.DueDate,
            PaidAt = i.PaidAt,
            VisitId = i.VisitId
        }).ToList();

        return new PagedResult<PatientInvoiceHistoryItemDto>
        {
            Items = items,
            TotalCount = paged.TotalCount,
            PageNumber = paged.PageNumber,
            PageSize = paged.PageSize
        };
    }
}
