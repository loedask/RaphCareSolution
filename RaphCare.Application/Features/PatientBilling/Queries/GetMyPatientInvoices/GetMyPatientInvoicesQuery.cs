using MediatR;
using RaphCare.Application.Common.DTOs;
using RaphCare.Application.Features.PatientBilling.DTOs;

namespace RaphCare.Application.Features.PatientBilling.Queries.GetMyPatientInvoices;

public sealed class GetMyPatientInvoicesQuery : IRequest<PagedResult<PatientInvoiceHistoryItemDto>>
{
    public int PageNumber { get; set; } = 1;
    public int PageSize { get; set; } = 20;
}
