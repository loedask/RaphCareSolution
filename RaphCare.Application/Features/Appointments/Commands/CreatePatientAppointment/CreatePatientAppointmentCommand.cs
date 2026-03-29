using MediatR;

namespace RaphCare.Application.Features.Appointments.Commands.CreatePatientAppointment;

public class CreatePatientAppointmentCommand : IRequest<Guid>
{
    public Guid ClinicId { get; set; }
    public Guid ProviderId { get; set; }
    public DateTime ScheduledStart { get; set; }
    public DateTime ScheduledEnd { get; set; }
    public string Type { get; set; } = string.Empty;
    public string Reason { get; set; } = string.Empty;
}
