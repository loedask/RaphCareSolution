using MediatR;
using RaphCare.Application.Features.MentalHealth.DTOs;

namespace RaphCare.Application.Features.MentalHealth.Commands.AddTherapyNote;

public sealed class AddTherapyNoteCommand : IRequest<TherapyNoteDto?>
{
    public Guid ClinicId { get; set; }
    public Guid SessionId { get; set; }
    public string Notes { get; set; } = string.Empty;
    public string Category { get; set; } = "Progress";
    public bool IsPrivate { get; set; } = true;
    public string? CrisisRiskLevel { get; set; }
    public string? CrisisDescription { get; set; }
}
