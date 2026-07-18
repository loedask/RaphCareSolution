using MediatR;
using RaphCare.Application.Features.Organization.DTOs;

namespace RaphCare.Application.Features.Organization.Commands.StartAdminClinicTeleSession;

public sealed class StartAdminClinicTeleSessionCommand : IRequest<AdminClinicTeleJoinInfoDto?>
{
    public Guid ClinicId { get; set; }
    public Guid AppointmentId { get; set; }
}
