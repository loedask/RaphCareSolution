using MediatR;
using RaphCare.Application.Features.Organization.DTOs;

namespace RaphCare.Application.Features.Organization.Commands.CreateAdminClinicWard;

public sealed class CreateAdminClinicWardCommand : IRequest<AdminClinicWardDto?>
{
    public Guid ClinicId { get; set; }
    public Guid FacilityId { get; set; }
    public string Name { get; set; } = string.Empty;
    public string? Code { get; set; }
}
