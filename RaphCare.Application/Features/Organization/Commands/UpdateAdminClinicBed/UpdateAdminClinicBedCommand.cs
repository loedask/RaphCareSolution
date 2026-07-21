using MediatR;
using RaphCare.Application.Features.Organization.DTOs;

namespace RaphCare.Application.Features.Organization.Commands.UpdateAdminClinicBed;

public sealed class UpdateAdminClinicBedCommand : IRequest<AdminClinicBedDto?>
{
    public Guid ClinicId { get; set; }
    public Guid BedId { get; set; }
    public string Label { get; set; } = string.Empty;
    public bool IsActive { get; set; } = true;
}
