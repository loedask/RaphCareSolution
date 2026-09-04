using MediatR;
using RaphCare.Application.Features.MentalHealth.DTOs;

namespace RaphCare.Application.Features.MentalHealth.Queries.GetMentalHealthInstrument;

public sealed class GetMentalHealthInstrumentQuery : IRequest<MentalHealthInstrumentDto?>
{
    public string AssessmentType { get; set; } = Phq9Instrument.AssessmentType;
}
