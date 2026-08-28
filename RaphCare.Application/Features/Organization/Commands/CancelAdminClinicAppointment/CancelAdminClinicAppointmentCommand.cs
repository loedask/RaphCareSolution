using MediatR;

namespace RaphCare.Application.Features.Organization.Commands.CancelAdminClinicAppointment;

public sealed class CancelAdminClinicAppointmentCommand : IRequest<bool>
{
    public Guid ClinicId { get; set; }
    public Guid AppointmentId { get; set; }
}
