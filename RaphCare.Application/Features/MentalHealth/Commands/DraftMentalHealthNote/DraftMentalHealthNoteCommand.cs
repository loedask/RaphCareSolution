using MediatR;
using RaphCare.Application.Features.MentalHealth.DTOs;

namespace RaphCare.Application.Features.MentalHealth.Commands.DraftMentalHealthNote;

public sealed class DraftMentalHealthNoteCommand : IRequest<MentalHealthNoteDraftDto?>
{
    public Guid ClinicId { get; set; }
    public Guid PatientId { get; set; }
}
