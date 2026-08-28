using MediatR;

namespace RaphCare.Application.Features.Organization.Commands.DeleteAdminFacility;

public sealed class DeleteAdminFacilityCommand : IRequest<bool>
{
    public Guid ClinicId { get; init; }
    public Guid FacilityId { get; init; }
}
