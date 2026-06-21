using MediatR;
using RaphCare.Application.Features.Organization.DTOs;

namespace RaphCare.Application.Features.Organization.Commands.GrantAdminClinicPatientAccess;

public sealed class GrantAdminClinicPatientAccessCommand : IRequest<AdminClinicPatientListItemDto?>
{
    public Guid ClinicId { get; set; }
    public string Email { get; set; } = string.Empty;
    public string? Notes { get; set; }
}
