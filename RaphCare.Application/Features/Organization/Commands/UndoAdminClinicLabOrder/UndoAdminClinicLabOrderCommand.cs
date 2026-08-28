using MediatR;
using RaphCare.Application.Features.Organization.DTOs;

namespace RaphCare.Application.Features.Organization.Commands.UndoAdminClinicLabOrder;

public sealed class UndoAdminClinicLabOrderCommand : IRequest<AdminClinicVisitLabResultDto?>
{
    public Guid ClinicId { get; set; }
    public Guid LabRequestId { get; set; }
}
