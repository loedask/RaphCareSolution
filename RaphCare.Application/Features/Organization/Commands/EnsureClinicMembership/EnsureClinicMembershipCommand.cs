using MediatR;

namespace RaphCare.Application.Features.Organization.Commands.EnsureClinicMembership;

public sealed class EnsureClinicMembershipCommand : IRequest<bool>
{
    public Guid ClinicId { get; set; }
}
