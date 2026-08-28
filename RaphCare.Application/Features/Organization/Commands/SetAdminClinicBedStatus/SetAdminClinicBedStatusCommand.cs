using MediatR;
using RaphCare.Application.Features.Organization.DTOs;

namespace RaphCare.Application.Features.Organization.Commands.SetAdminClinicBedStatus;

public sealed class SetAdminClinicBedStatusCommand : IRequest<AdminClinicBedDto?>
{
    public Guid ClinicId { get; set; }
    public Guid BedId { get; set; }
    public string Status { get; set; } = string.Empty;
}
