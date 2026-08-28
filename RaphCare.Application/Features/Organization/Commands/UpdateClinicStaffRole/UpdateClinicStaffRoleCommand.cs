using MediatR;

namespace RaphCare.Application.Features.Organization.Commands.UpdateClinicStaffRole;

public sealed class UpdateClinicStaffRoleCommand : IRequest<bool>
{
    public Guid ClinicId { get; init; }
    public Guid UserId { get; init; }
    public bool IsAdministrator { get; init; }
    public string? JobRole { get; init; }
}
