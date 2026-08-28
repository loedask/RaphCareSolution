using MediatR;

namespace RaphCare.Application.Features.Organization.Commands.DeleteAdminClinicBed;

public sealed class DeleteAdminClinicBedCommand : IRequest<bool>
{
    public Guid ClinicId { get; set; }
    public Guid BedId { get; set; }
}
