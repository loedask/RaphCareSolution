using MediatR;
using RaphCare.Application.Features.Organization.DTOs;

namespace RaphCare.Application.Features.Organization.Commands.CreateAdminFacility;

public sealed class CreateAdminFacilityCommand : IRequest<FacilityListItemDto?>
{
    public Guid ClinicId { get; init; }
    public string Name { get; init; } = string.Empty;
    public string Address { get; init; } = string.Empty;
    public string City { get; init; } = string.Empty;
    public string Country { get; init; } = string.Empty;
    public bool IsVirtual { get; init; }
}
