using MediatR;
using RaphCare.Application.Common.DTOs;
using RaphCare.Application.Features.PatientHealthRecords.DTOs;

namespace RaphCare.Application.Features.PatientHealthRecords.Queries.GetMyHealthRecords;

public class GetMyHealthRecordsQuery : IRequest<PagedResult<PatientHealthRecordListItemDto>>
{
    public int PageNumber { get; set; } = 1;
    public int PageSize { get; set; } = 20;
}
