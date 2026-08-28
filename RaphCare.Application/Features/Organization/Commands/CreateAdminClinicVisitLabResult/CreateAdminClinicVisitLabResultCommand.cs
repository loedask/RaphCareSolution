using MediatR;
using RaphCare.Application.Features.Organization.DTOs;

namespace RaphCare.Application.Features.Organization.Commands.CreateAdminClinicVisitLabResult;

public sealed class CreateAdminClinicVisitLabResultCommand : IRequest<AdminClinicVisitLabResultDto?>
{
    public Guid ClinicId { get; set; }
    public Guid VisitId { get; set; }
    public string TestName { get; set; } = string.Empty;
    public string? Priority { get; set; }
}
