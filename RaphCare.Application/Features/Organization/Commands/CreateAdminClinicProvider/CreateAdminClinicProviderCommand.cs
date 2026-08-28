using MediatR;
using RaphCare.Application.Features.Organization.DTOs;

namespace RaphCare.Application.Features.Organization.Commands.CreateAdminClinicProvider;

public sealed class CreateAdminClinicProviderCommand : IRequest<AdminClinicProviderListItemDto?>
{
    public Guid ClinicId { get; set; }
    public Guid UserId { get; set; }
    public string? LicenseNumber { get; set; }
}
