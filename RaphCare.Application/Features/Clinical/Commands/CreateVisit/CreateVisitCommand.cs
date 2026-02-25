using MediatR;

namespace RaphCare.Application.Features.Clinical.Commands.CreateVisit;

public class CreateVisitCommand : IRequest<Guid>
{
    public Guid ClinicId { get; set; }
    public Guid AppointmentId { get; set; }
    public Guid PatientId { get; set; }
    public Guid ProviderId { get; set; }
    public DateTime VisitStart { get; set; }
    public string VisitType { get; set; } = string.Empty;
    public string? Summary { get; set; }
}

