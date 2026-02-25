using MediatR;

namespace RaphCare.Application.Features.Appointments.Commands.UpdateAppointment;

public class UpdateAppointmentCommand : IRequest
{
    public Guid Id { get; set; }
    public DateTime? ScheduledStart { get; set; }
    public DateTime? ScheduledEnd { get; set; }
    public string? Status { get; set; }
    public string? Reason { get; set; }
}

