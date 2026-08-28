using MediatR;
using RaphCare.Application.Common.Interfaces;
using RaphCare.Application.Features.PatientSupport.DTOs;

namespace RaphCare.Application.Features.PatientSupport.Queries.GetPatientSupportContent;

public sealed class GetPatientSupportContentHandler(IPatientSupportContentProvider content)
    : IRequestHandler<GetPatientSupportContentQuery, PatientSupportContentDto>
{
    private readonly IPatientSupportContentProvider _content = content;

    public Task<PatientSupportContentDto> Handle(
        GetPatientSupportContentQuery request,
        CancellationToken cancellationToken) =>
        _content.GetContentAsync(cancellationToken);
}
