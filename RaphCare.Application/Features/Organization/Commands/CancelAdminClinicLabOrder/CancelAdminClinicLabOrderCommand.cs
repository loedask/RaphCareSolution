using MediatR;
using RaphCare.Application.Features.Organization.DTOs;

namespace RaphCare.Application.Features.Organization.Commands.CancelAdminClinicLabOrder;

public sealed class CancelAdminClinicLabOrderCommand : IRequest<AdminClinicVisitLabResultDto?>
{
    public Guid ClinicId { get; set; }
    public Guid LabRequestId { get; set; }
}
