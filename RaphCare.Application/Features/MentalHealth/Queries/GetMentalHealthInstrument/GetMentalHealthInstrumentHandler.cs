using MediatR;
using RaphCare.Application.Features.MentalHealth.DTOs;

namespace RaphCare.Application.Features.MentalHealth.Queries.GetMentalHealthInstrument;

public sealed class GetMentalHealthInstrumentHandler
    : IRequestHandler<GetMentalHealthInstrumentQuery, MentalHealthInstrumentDto?>
{
    public Task<MentalHealthInstrumentDto?> Handle(
        GetMentalHealthInstrumentQuery request,
        CancellationToken cancellationToken) =>
        Task.FromResult(MentalHealthInstruments.GetDto(request.AssessmentType));
}
