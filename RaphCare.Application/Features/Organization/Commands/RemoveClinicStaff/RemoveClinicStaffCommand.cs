using MediatR;

namespace RaphCare.Application.Features.Organization.Commands.RemoveClinicStaff;

public sealed class RemoveClinicStaffCommand : IRequest<bool>
{
    public Guid ClinicId { get; init; }
    public Guid UserId { get; init; }
}
