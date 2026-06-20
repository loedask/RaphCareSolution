using MediatR;

namespace RaphCare.Application.Features.Organization.Commands.ResendClinicStaffInvitation;

public sealed class ResendClinicStaffInvitationCommand : IRequest<bool>
{
    public Guid ClinicId { get; init; }
    public Guid UserId { get; init; }
}
