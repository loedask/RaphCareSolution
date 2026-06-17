using MediatR;
using RaphCare.Application.Common.DTOs;
using RaphCare.Application.Common.Interfaces;
using RaphCare.Application.Features.Clinical.DTOs;
using RaphCare.Domain.Clinical;

namespace RaphCare.Application.Features.Clinical.Queries.GetVisits;

public class GetVisitsHandler : IRequestHandler<GetVisitsQuery, PagedResult<VisitDto>>
{
    private readonly IRepository<Visit> _repository;

    public GetVisitsHandler(IRepository<Visit> repository)
    {
        _repository = repository;
    }

    public async Task<PagedResult<VisitDto>> Handle(GetVisitsQuery request, CancellationToken cancellationToken)
    {
        var visits = await _repository.ListAsync(cancellationToken);

        var totalCount = visits.Count;

        var items = visits
            .Skip((request.PageNumber - 1) * request.PageSize)
            .Take(request.PageSize)
            .Select(v => new VisitDto
            {
                Id = v.Id,
                ClinicId = v.ClinicId,
                AppointmentId = v.AppointmentId,
                PatientId = v.PatientId,
                ProviderId = v.ProviderId,
                VisitStart = v.VisitStart,
                VisitEnd = v.VisitEnd,
                VisitType = v.VisitType,
                Status = v.Status
            })
            .ToList();

        return new PagedResult<VisitDto>
        {
            Items = items,
            TotalCount = totalCount,
            PageNumber = request.PageNumber,
            PageSize = request.PageSize
        };
    }
}

