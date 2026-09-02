using MediatR;
using RaphCare.Application.Features.Organization.DTOs;

namespace RaphCare.Application.Features.Organization.Commands.UpdateAdminClinicTheatreCaseStatus;

public sealed class UpdateAdminClinicTheatreCaseStatusCommand : IRequest<AdminClinicTheatreCaseDto?>
{
    public Guid ClinicId { get; set; }
    public Guid CaseId { get; set; }
    public string Status { get; set; } = string.Empty;
}
