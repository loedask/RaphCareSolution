using MediatR;
using RaphCare.Application.Features.Organization.DTOs;

namespace RaphCare.Application.Features.Organization.Commands.UpdateAdminFacility;

public sealed class UpdateAdminFacilityCommand : IRequest<FacilityListItemDto?>
{
    public Guid ClinicId { get; init; }
    public Guid FacilityId { get; init; }
    public string Name { get; init; } = string.Empty;
    public string Address { get; init; } = string.Empty;
    public string City { get; init; } = string.Empty;
    public string Country { get; init; } = string.Empty;
    public bool IsVirtual { get; init; }
}
