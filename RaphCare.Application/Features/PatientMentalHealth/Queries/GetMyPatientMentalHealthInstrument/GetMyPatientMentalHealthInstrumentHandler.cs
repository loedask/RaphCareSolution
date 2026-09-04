using MediatR;
using RaphCare.Application.Features.MentalHealth;
using RaphCare.Application.Features.MentalHealth.DTOs;

namespace RaphCare.Application.Features.PatientMentalHealth.Queries.GetMyPatientMentalHealthInstrument;

public sealed class GetMyPatientMentalHealthInstrumentHandler
    : IRequestHandler<GetMyPatientMentalHealthInstrumentQuery, MentalHealthInstrumentDto?>
{
    public Task<MentalHealthInstrumentDto?> Handle(
        GetMyPatientMentalHealthInstrumentQuery request,
        CancellationToken cancellationToken) =>
        Task.FromResult(MentalHealthInstruments.GetDto(request.AssessmentType));
}
