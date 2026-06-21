using MediatR;
using RaphCare.Application.Features.Organization.DTOs;

namespace RaphCare.Application.Features.Organization.Commands.RescheduleAdminClinicAppointment;

public sealed class RescheduleAdminClinicAppointmentCommand : IRequest<AdminClinicAppointmentListItemDto?>
{
    public Guid ClinicId { get; set; }
    public Guid AppointmentId { get; set; }
    public Guid? ProviderId { get; set; }
    public DateTime ScheduledStart { get; set; }
    public DateTime ScheduledEnd { get; set; }
    public string? Reason { get; set; }
}
