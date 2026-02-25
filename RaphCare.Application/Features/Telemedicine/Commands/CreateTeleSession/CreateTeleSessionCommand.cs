using MediatR;

namespace RaphCare.Application.Features.Telemedicine.Commands.CreateTeleSession;

public class CreateTeleSessionCommand : IRequest<Guid>
{
    public Guid ClinicId { get; set; }
    public Guid AppointmentId { get; set; }
    public Guid VisitId { get; set; }
    public Guid PatientId { get; set; }
    public Guid ProviderId { get; set; }
    public DateTime ScheduledStart { get; set; }
    public string Platform { get; set; } = string.Empty;
}

