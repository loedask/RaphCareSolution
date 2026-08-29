using MediatR;
using RaphCare.Application.Features.Organization.DTOs;

namespace RaphCare.Application.Features.Organization.Commands.CallAdminClinicLabOrder;

public sealed class CallAdminClinicLabOrderCommand : IRequest<AdminClinicVisitLabResultDto?>
{
    public Guid ClinicId { get; set; }
    public Guid LabRequestId { get; set; }
}
