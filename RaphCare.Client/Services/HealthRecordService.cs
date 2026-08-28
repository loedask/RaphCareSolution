using RaphCare.Client.Contracts;
using RaphCare.Client.Contracts.Interfaces;
using RaphCare.Client.Models.Api;
using RaphCare.Client.Models.HealthRecords;
using RaphCare.Client.Services.Base;

namespace RaphCare.Client.Services;

public sealed class HealthRecordService(HttpClient httpClient) : BaseHttpService(httpClient), IHealthRecordService
{
    public async Task<Response<PagedHealthRecordsViewModel>> GetMyHealthRecordsAsync(
        int pageNumber = 1,
        int pageSize = 20,
        CancellationToken cancellationToken = default)
    {
        var result = await GetAsync<PagedApiResult<HealthRecordListItemDto>>(
                $"api/patient/health-records?pageNumber={pageNumber}&pageSize={pageSize}",
                cancellationToken)
            .ConfigureAwait(false);

        if (!result.IsSuccess || result.Data is null)
            return Response<PagedHealthRecordsViewModel>.Failure(result.ErrorMessage ?? "Could not load records.", result.StatusCode);

        return Response<PagedHealthRecordsViewModel>.Success(MapPaged(result.Data));
    }

    public async Task<Response<HealthRecordDetailViewModel?>> GetMyHealthRecordAsync(Guid id, CancellationToken cancellationToken = default)
    {
        var result = await GetAsync<HealthRecordDetailDto>($"api/patient/health-records/{id}", cancellationToken).ConfigureAwait(false);
        if (!result.IsSuccess)
            return Response<HealthRecordDetailViewModel?>.Failure(result.ErrorMessage ?? "Could not load record.", result.StatusCode);
        return Response<HealthRecordDetailViewModel?>.Success(result.Data is null ? null : MapDetail(result.Data));
    }

    private static PagedHealthRecordsViewModel MapPaged(PagedApiResult<HealthRecordListItemDto> paged)
    {
        var items = (paged.Items ?? Array.Empty<HealthRecordListItemDto>())
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

    private static HealthRecordDetailViewModel MapDetail(HealthRecordDetailDto dto)
    {
        var vitals = (dto.VitalSigns ?? Array.Empty<VitalSignDto>())
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

    private sealed class HealthRecordListItemDto
    {
        public Guid Id { get; set; }
        public DateTime VisitStart { get; set; }
        public DateTime? VisitEnd { get; set; }
        public string? VisitType { get; set; }
        public string? Status { get; set; }
        public string? Summary { get; set; }
    }

    private sealed class HealthRecordDetailDto
    {
        public Guid Id { get; set; }
        public DateTime VisitStart { get; set; }
        public DateTime? VisitEnd { get; set; }
        public string? VisitType { get; set; }
        public string? Status { get; set; }
        public string? Summary { get; set; }
        public IReadOnlyList<VitalSignDto>? VitalSigns { get; set; }
    }

    private sealed class VitalSignDto
    {
        public string? Type { get; set; }
        public double Value { get; set; }
        public string? Unit { get; set; }
        public DateTime RecordedAt { get; set; }
    }
}
