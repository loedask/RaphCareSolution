using MediatR;

namespace RaphCare.Application.Features.Organization.Commands.EnsureClinicMembership;

public sealed class ClaimClinicByRegistrationNumberCommand : IRequest<Guid?>
{
    public string RegistrationNumber { get; set; } = string.Empty;
}
