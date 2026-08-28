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

    public async Task<Response<PatientCollectionOrdersViewModel>> GetMyCollectionOrdersAsync(CancellationToken cancellationToken = default)
    {
        var result = await GetAsync<CollectionOrdersDto>("api/patient/collection-orders", cancellationToken).ConfigureAwait(false);
        if (!result.IsSuccess || result.Data is null)
            return Response<PatientCollectionOrdersViewModel>.Failure(result.ErrorMessage ?? "Could not load items to collect.", result.StatusCode);

        return Response<PatientCollectionOrdersViewModel>.Success(MapCollection(result.Data));
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
            VitalSigns = vitals,
            Prescriptions = MapPrescriptions(dto.Prescriptions),
            LabOrders = MapLabs(dto.LabOrders)
        };
    }

    private static PatientCollectionOrdersViewModel MapCollection(CollectionOrdersDto dto) => new()
    {
        Prescriptions = MapPrescriptions(dto.Prescriptions),
        LabOrders = MapLabs(dto.LabOrders)
    };

    private static List<PatientCollectionPrescriptionViewModel> MapPrescriptions(
        IReadOnlyList<CollectionPrescriptionDto>? items) =>
        (items ?? Array.Empty<CollectionPrescriptionDto>())
        .Select(p => new PatientCollectionPrescriptionViewModel
        {
            Id = p.Id,
            VisitId = p.VisitId,
            ClinicName = p.ClinicName ?? string.Empty,
            PickupCode = p.PickupCode ?? string.Empty,
            IssuedAt = p.IssuedAt,
            Status = p.Status ?? string.Empty,
            Notes = p.Notes,
            Items = (p.Items ?? Array.Empty<CollectionPrescriptionItemDto>())
                .Select(i => new PatientCollectionPrescriptionItemViewModel
                {
                    MedicationName = i.MedicationName ?? string.Empty,
                    Dosage = i.Dosage,
                    Frequency = i.Frequency,
                    DurationDays = i.DurationDays
                })
                .ToList()
        })
        .ToList();

    private static List<PatientCollectionLabOrderViewModel> MapLabs(
        IReadOnlyList<CollectionLabOrderDto>? items) =>
        (items ?? Array.Empty<CollectionLabOrderDto>())
        .Select(l => new PatientCollectionLabOrderViewModel
        {
            Id = l.Id,
            VisitId = l.VisitId,
            ClinicName = l.ClinicName ?? string.Empty,
            PickupCode = l.PickupCode ?? string.Empty,
            TestName = l.TestName ?? string.Empty,
            Status = l.Status ?? string.Empty,
            RequestedAt = l.RequestedAt,
            ResultValue = l.ResultValue,
            Unit = l.Unit,
            ReferenceRange = l.ReferenceRange,
            ReportedAt = l.ReportedAt
        })
        .ToList();

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
        public IReadOnlyList<CollectionPrescriptionDto>? Prescriptions { get; set; }
        public IReadOnlyList<CollectionLabOrderDto>? LabOrders { get; set; }
    }

    private sealed class CollectionOrdersDto
    {
        public IReadOnlyList<CollectionPrescriptionDto>? Prescriptions { get; set; }
        public IReadOnlyList<CollectionLabOrderDto>? LabOrders { get; set; }
    }

    private sealed class CollectionPrescriptionDto
    {
        public Guid Id { get; set; }
        public Guid VisitId { get; set; }
        public string? ClinicName { get; set; }
        public string? PickupCode { get; set; }
        public DateTime IssuedAt { get; set; }
        public string? Status { get; set; }
        public string? Notes { get; set; }
        public IReadOnlyList<CollectionPrescriptionItemDto>? Items { get; set; }
    }

    private sealed class CollectionPrescriptionItemDto
    {
        public string? MedicationName { get; set; }
        public string? Dosage { get; set; }
        public string? Frequency { get; set; }
        public int DurationDays { get; set; }
    }

    private sealed class CollectionLabOrderDto
    {
        public Guid Id { get; set; }
        public Guid VisitId { get; set; }
        public string? ClinicName { get; set; }
        public string? PickupCode { get; set; }
        public string? TestName { get; set; }
        public string? Status { get; set; }
        public DateTime RequestedAt { get; set; }
        public string? ResultValue { get; set; }
        public string? Unit { get; set; }
        public string? ReferenceRange { get; set; }
        public DateTime? ReportedAt { get; set; }
    }

    private sealed class VitalSignDto
    {
        public string? Type { get; set; }
        public double Value { get; set; }
        public string? Unit { get; set; }
        public DateTime RecordedAt { get; set; }
    }
}
