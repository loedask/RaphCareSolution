using MediatR;
using RaphCare.Application.Features.MentalHealth.DTOs;

namespace RaphCare.Application.Features.MentalHealth.Commands.CreateTherapySession;

public sealed class CreateTherapySessionCommand : IRequest<TherapySessionDto?>
{
    public Guid ClinicId { get; set; }
    public Guid PatientId { get; set; }
    public string SessionType { get; set; } = "Individual";
    public string? Summary { get; set; }
    public bool IsConfidential { get; set; } = true;
}
