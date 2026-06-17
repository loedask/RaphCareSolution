using RaphCare.Client.Contracts;
using RaphCare.Client.Contracts.Interfaces;
using RaphCare.Client.Models.HealthRecords;
using RaphCare.Client.Services.Base;

namespace RaphCare.Client.Services;

/// <summary>Wraps generated <see cref="IClient"/> health-record operations and maps to feature view models.</summary>
public sealed class HealthRecordService(IClient client) : IHealthRecordService
{
    public async Task<Response<PagedHealthRecordsViewModel>> GetMyHealthRecordsAsync(int pageNumber = 1, int pageSize = 20, CancellationToken cancellationToken = default)
    {
        try
        {
            var paged = await client.GetMyHealthRecordsAsync(pageNumber, pageSize, cancellationToken).ConfigureAwait(false);
            return Response<PagedHealthRecordsViewModel>.Success(MapPaged(paged));
        }
        catch (global::RaphCare.Client.Services.Base.ApiException ex)
        {
            return Response<PagedHealthRecordsViewModel>.Failure(ex.Message, ex.StatusCode);
        }
    }

    public async Task<Response<HealthRecordDetailViewModel?>> GetMyHealthRecordAsync(Guid id, CancellationToken cancellationToken = default)
    {
        try
        {
            var dto = await client.GetMyHealthRecordByIdAsync(id, cancellationToken).ConfigureAwait(false);
            return Response<HealthRecordDetailViewModel?>.Success(MapDetail(dto));
        }
        catch (global::RaphCare.Client.Services.Base.ApiException ex)
        {
            return Response<HealthRecordDetailViewModel?>.Failure(ex.Message, ex.StatusCode);
        }
    }

    private static PagedHealthRecordsViewModel MapPaged(PatientHealthRecordListItemDtoPagedResult paged)
    {
        var items = (paged.Items ?? Array.Empty<PatientHealthRecordListItemDto>())
            .Select(i => new HealthRecordListItemViewModel
            {
                Id = i.Id,
                VisitStart = i.VisitStart,
                VisitEnd = i.VisitEnd,
                VisitType = i.VisitType ?? string.Empty,
                Status = i.Status ?? string.Empty,
                Summary = i.Summary
            })
            .ToList();

        return new PagedHealthRecordsViewModel
        {
            Items = items,
            TotalCount = paged.TotalCount,
            PageNumber = paged.PageNumber,
            PageSize = paged.PageSize
        };
    }

    private static HealthRecordDetailViewModel MapDetail(PatientHealthRecordDetailDto dto)
    {
        var vitals = (dto.VitalSigns ?? Array.Empty<PatientVitalSignDto>())
            .Select(v => new VitalSignViewModel
            {
                Type = v.Type ?? string.Empty,
                Value = (decimal)v.Value,
                Unit = v.Unit,
                RecordedAt = v.RecordedAt
            })
            .ToList();

        return new HealthRecordDetailViewModel
        {
            Id = dto.Id,
            VisitStart = dto.VisitStart,
            VisitEnd = dto.VisitEnd,
            VisitType = dto.VisitType ?? string.Empty,
            Status = dto.Status ?? string.Empty,
            Summary = dto.Summary,
            VitalSigns = vitals
        };
    }
}
