using MediatR;
using RaphCare.Application.Features.MentalHealth.DTOs;

namespace RaphCare.Application.Features.PatientMentalHealth.Queries.GetMyPatientMentalHealthInstrument;

public sealed class GetMyPatientMentalHealthInstrumentQuery : IRequest<MentalHealthInstrumentDto?>
{
    public string AssessmentType { get; set; } = "PHQ-9";
}
