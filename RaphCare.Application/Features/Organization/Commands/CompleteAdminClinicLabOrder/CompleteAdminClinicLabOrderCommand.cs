using MediatR;
using RaphCare.Application.Features.Organization.DTOs;

namespace RaphCare.Application.Features.Organization.Commands.CompleteAdminClinicLabOrder;

public sealed class CompleteAdminClinicLabOrderCommand : IRequest<AdminClinicVisitLabResultDto?>
{
    public Guid ClinicId { get; set; }
    public Guid LabRequestId { get; set; }
    public string ResultValue { get; set; } = string.Empty;
    public string? Unit { get; set; }
    public string? ReferenceRange { get; set; }
}
