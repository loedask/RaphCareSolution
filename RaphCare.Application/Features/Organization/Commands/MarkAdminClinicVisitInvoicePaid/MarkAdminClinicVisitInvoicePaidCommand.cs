using MediatR;
using RaphCare.Application.Features.Organization.DTOs;

namespace RaphCare.Application.Features.Organization.Commands.MarkAdminClinicVisitInvoicePaid;

public sealed class MarkAdminClinicVisitInvoicePaidCommand : IRequest<AdminClinicDaySheetItemDto?>
{
    public Guid ClinicId { get; set; }
    public Guid InvoiceId { get; set; }
}
