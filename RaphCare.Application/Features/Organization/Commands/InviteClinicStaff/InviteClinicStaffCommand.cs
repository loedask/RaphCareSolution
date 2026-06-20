using MediatR;
using RaphCare.Application.Features.Organization.DTOs;

namespace RaphCare.Application.Features.Organization.Commands.InviteClinicStaff;

public sealed class InviteClinicStaffCommand : IRequest<ClinicStaffMemberDto?>
{
    public Guid ClinicId { get; init; }
    public string Email { get; init; } = string.Empty;
}
