using System.Linq;
using MediatR;
using RaphCare.Application.Common.DTOs;
using RaphCare.Application.Common.Exceptions;
using RaphCare.Application.Common.Interfaces;
using RaphCare.Application.Features.PatientHealthRecords.DTOs;
using RaphCare.Domain.Clinical;

namespace RaphCare.Application.Features.PatientHealthRecords.Queries.GetMyHealthRecords;

public class GetMyHealthRecordsHandler : IRequestHandler<GetMyHealthRecordsQuery, PagedResult<PatientHealthRecordListItemDto>>
{
    private readonly IRepository<Visit> _repository;
    private readonly ICurrentUserService _currentUser;

    public GetMyHealthRecordsHandler(IRepository<Visit> repository, ICurrentUserService currentUser)
    {
        _repository = repository;
        _currentUser = currentUser;
    }

    public async Task<PagedResult<PatientHealthRecordListItemDto>> Handle(GetMyHealthRecordsQuery request, CancellationToken cancellationToken)
    {
        var patientId = _currentUser.CurrentPatientId
            ?? throw new ForbiddenAccessException("A patient profile is required to view health records.");

        var paged = await _repository.SearchAsync(
            q => q.Where(v => v.PatientId == patientId).OrderByDescending(v => v.VisitStart),
            request.PageNumber,
            request.PageSize,
            applyDefaultIdOrdering: false,
            cancellationToken).ConfigureAwait(false);

        var items = paged.Items.Select(v => new PatientHealthRecordListItemDto
        {
            Id = v.Id,
            VisitStart = v.VisitStart,
            VisitEnd = v.VisitEnd,
            VisitType = v.VisitType,
            Status = v.Status,
            Summary = v.Summary
        }).ToList();

        return new PagedResult<PatientHealthRecordListItemDto>
        {
            Items = items,
            TotalCount = paged.TotalCount,
            PageNumber = paged.PageNumber,
            PageSize = paged.PageSize
        };
    }
}
