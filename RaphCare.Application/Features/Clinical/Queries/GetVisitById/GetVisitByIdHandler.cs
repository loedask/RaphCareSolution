using MediatR;
using RaphCare.Application.Common.Exceptions;
using RaphCare.Application.Common.Interfaces;
using RaphCare.Application.Features.Clinical.DTOs;
using RaphCare.Domain.Clinical;

namespace RaphCare.Application.Features.Clinical.Queries.GetVisitById;

public class GetVisitByIdHandler : IRequestHandler<GetVisitByIdQuery, VisitDto>
{
    private readonly IRepository<Visit> _repository;

    public GetVisitByIdHandler(IRepository<Visit> repository)
    {
        _repository = repository;
    }

    public async Task<VisitDto> Handle(GetVisitByIdQuery request, CancellationToken cancellationToken)
    {
        var visit = await _repository.GetByIdAsync(request.Id, cancellationToken);
        if (visit is null)
        {
            throw new NotFoundException(nameof(Visit), request.Id);
        }

        return new VisitDto
        {
            Id = visit.Id,
            ClinicId = visit.ClinicId,
            AppointmentId = visit.AppointmentId,
            PatientId = visit.PatientId,
            ProviderId = visit.ProviderId,
            VisitStart = visit.VisitStart,
            VisitEnd = visit.VisitEnd,
            VisitType = visit.VisitType,
            Status = visit.Status
        };
    }
}

