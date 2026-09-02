using MediatR;
using RaphCare.Application.Common.DTOs;
using RaphCare.Application.Common.Exceptions;
using RaphCare.Application.Common.Interfaces;
using RaphCare.Application.Features.PatientHealthRecords.DTOs;
using RaphCare.Domain.Clinical;

namespace RaphCare.Application.Features.PatientHealthRecords.Queries.GetMyHealthRecords;

public class GetMyHealthRecordsHandler(
    IRepository<Visit> visitRepository,
    IRepository<InpatientAdmission> admissionRepository,
    ICurrentUserService currentUser)
    : IRequestHandler<GetMyHealthRecordsQuery, PagedResult<PatientHealthRecordListItemDto>>
{
    public async Task<PagedResult<PatientHealthRecordListItemDto>> Handle(
        GetMyHealthRecordsQuery request,
        CancellationToken cancellationToken)
    {
        var patientId = currentUser.CurrentPatientId
            ?? throw new ForbiddenAccessException("A patient profile is required to view health records.");

        var visits = await visitRepository.SearchAsync(
            q => q.Where(v => v.PatientId == patientId),
            1,
            1000,
            applyDefaultIdOrdering: false,
            cancellationToken).ConfigureAwait(false);

        var stays = await admissionRepository.SearchAsync(
            q => q.Where(a => a.PatientId == patientId && a.Status == "Discharged"),
            1,
            1000,
            applyDefaultIdOrdering: false,
            cancellationToken).ConfigureAwait(false);

        var items = visits.Items.Select(v => new PatientHealthRecordListItemDto
        {
            Id = v.Id,
            VisitStart = v.VisitStart,
            VisitEnd = v.VisitEnd,
            VisitType = v.VisitType,
            Status = v.Status,
            Summary = v.Summary,
            RecordKind = "Visit"
        }).Concat(stays.Items.Select(a => new PatientHealthRecordListItemDto
        {
            Id = a.Id,
            VisitStart = a.AdmittedAt,
            VisitEnd = a.DischargedAt,
            VisitType = "Stay",
            Status = a.Status,
            Summary = a.DischargeSummary,
            RecordKind = "Discharge"
        }))
            .OrderByDescending(i => i.VisitStart)
            .ToList();

        var pageNumber = request.PageNumber < 1 ? 1 : request.PageNumber;
        var pageSize = request.PageSize < 1 ? 20 : request.PageSize;
        var page = items.Skip((pageNumber - 1) * pageSize).Take(pageSize).ToList();

        return new PagedResult<PatientHealthRecordListItemDto>
        {
            Items = page,
            TotalCount = items.Count,
            PageNumber = pageNumber,
            PageSize = pageSize
        };
    }
}
