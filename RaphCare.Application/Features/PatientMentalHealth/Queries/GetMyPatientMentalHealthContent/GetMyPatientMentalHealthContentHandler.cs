using MediatR;
using RaphCare.Application.Common.Interfaces;
using RaphCare.Application.Features.PatientMentalHealth.DTOs;

namespace RaphCare.Application.Features.PatientMentalHealth.Queries.GetMyPatientMentalHealthContent;

public sealed class GetMyPatientMentalHealthContentHandler(IPatientMentalHealthContentProvider content)
    : IRequestHandler<GetMyPatientMentalHealthContentQuery, PatientMentalHealthContentDto>
{
    private readonly IPatientMentalHealthContentProvider _content = content;

    public Task<PatientMentalHealthContentDto> Handle(
        GetMyPatientMentalHealthContentQuery request,
        CancellationToken cancellationToken) =>
        _content.GetContentAsync(cancellationToken);
}
