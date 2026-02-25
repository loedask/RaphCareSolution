using MediatR;

namespace RaphCare.Application.Features.Appointments.Commands.CreateAppointment;

public class CreateAppointmentCommand : IRequest<Guid>
{
    public Guid ClinicId { get; set; }
    public Guid PatientId { get; set; }
    public Guid ProviderId { get; set; }
    public DateTime ScheduledStart { get; set; }
    public DateTime ScheduledEnd { get; set; }
    public string Type { get; set; } = string.Empty;
    public string Reason { get; set; } = string.Empty;
}

