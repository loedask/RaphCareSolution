using MediatR;
using RaphCare.Application.Features.Organization.DTOs;
using RaphCare.Domain.Identity;

namespace RaphCare.Application.Features.Organization.Commands.InviteClinicStaff;

public sealed class InviteClinicStaffCommand : IRequest<ClinicStaffMemberDto?>
{
    public Guid ClinicId { get; init; }
    public string Email { get; init; } = string.Empty;
    public string JobRole { get; init; } = RaphCareRoles.Clinician;
}
