using RaphCare.Client.Contracts;
using RaphCare.Client.Contracts.Interfaces;
using RaphCare.Client.Models.Api;
using RaphCare.Client.Models.Clinical;
using RaphCare.Client.Services.Base;

namespace RaphCare.Client.Services;

public sealed class ClinicalPatientDeviceReadingsService(HttpClient httpClient)
    : BaseHttpService(httpClient), IClinicalPatientDeviceReadingsService
{
    public async Task<Response<PagedPatientDeviceReadingsViewModel>> GetReadingsAsync(
        Guid patientId,
        int pageNumber = 1,
        int pageSize = 20,
        string? readingType = null,
        DateTime? recordedFromUtc = null,
        DateTime? recordedToUtc = null,
        CancellationToken cancellationToken = default)
    {
        var query = BuildQuery(pageNumber, pageSize, readingType, recordedFromUtc, recordedToUtc);
        var result = await GetAsync<PagedApiResult<DeviceReadingDto>>(
                $"api/Clinical/patients/{patientId}/device-readings{query}",
                cancellationToken)
            .ConfigureAwait(false);

        if (!result.IsSuccess || result.Data is null)
            return Response<PagedPatientDeviceReadingsViewModel>.Failure(result.ErrorMessage ?? "Could not load readings.", result.StatusCode);

        var dto = result.Data;
        return Response<PagedPatientDeviceReadingsViewModel>.Success(new PagedPatientDeviceReadingsViewModel
        {
            Items = (dto.Items ?? Array.Empty<DeviceReadingDto>()).Select(MapReading).ToList(),
            TotalCount = dto.TotalCount,
            PageNumber = dto.PageNumber,
            PageSize = dto.PageSize
        });
    }

    public async Task<Response<IReadOnlyList<DeviceReadingDailyRollupViewModel>>> GetDailyRollupAsync(
        Guid patientId,
        DateTime fromUtc,
        DateTime toUtc,
        CancellationToken cancellationToken = default)
    {
        var from = Uri.EscapeDataString(fromUtc.ToString("O"));
        var to = Uri.EscapeDataString(toUtc.ToString("O"));
        var result = await GetAsync<IReadOnlyList<DailyRollupDto>>(
                $"api/Clinical/patients/{patientId}/device-readings/daily-rollup?fromUtc={from}&toUtc={to}",
                cancellationToken)
            .ConfigureAwait(false);

        if (!result.IsSuccess)
            return Response<IReadOnlyList<DeviceReadingDailyRollupViewModel>>.Failure(result.ErrorMessage ?? "Could not load rollup.", result.StatusCode);

        var list = (result.Data ?? Array.Empty<DailyRollupDto>()).Select(MapRollup).ToList();
        return Response<IReadOnlyList<DeviceReadingDailyRollupViewModel>>.Success(list);
    }

    public async Task<Response<PagedPatientDeviceEmergencyEventsViewModel>> GetEmergencyEventsAsync(
        Guid patientId,
        int pageNumber = 1,
        int pageSize = 20,
        DateTime? occurredFromUtc = null,
        DateTime? occurredToUtc = null,
        CancellationToken cancellationToken = default)
    {
        var query = BuildEmergencyQuery(pageNumber, pageSize, occurredFromUtc, occurredToUtc);
        var result = await GetAsync<PagedApiResult<EmergencyEventDto>>(
                $"api/Clinical/patients/{patientId}/emergency-events{query}",
                cancellationToken)
            .ConfigureAwait(false);

        if (!result.IsSuccess || result.Data is null)
            return Response<PagedPatientDeviceEmergencyEventsViewModel>.Failure(result.ErrorMessage ?? "Could not load events.", result.StatusCode);

        var dto = result.Data;
        return Response<PagedPatientDeviceEmergencyEventsViewModel>.Success(new PagedPatientDeviceEmergencyEventsViewModel
        {
            Items = (dto.Items ?? Array.Empty<EmergencyEventDto>()).Select(MapEmergency).ToList(),
            TotalCount = dto.TotalCount,
            PageNumber = dto.PageNumber,
            PageSize = dto.PageSize
        });
    }

    private static string BuildQuery(int pageNumber, int pageSize, string? readingType, DateTime? from, DateTime? to)
    {
        var parts = new List<string>
        {
            $"pageNumber={pageNumber}",
            $"pageSize={pageSize}"
        };
        if (!string.IsNullOrEmpty(readingType))
            parts.Add($"readingType={Uri.EscapeDataString(readingType)}");
        if (from.HasValue)
            parts.Add($"recordedFromUtc={Uri.EscapeDataString(from.Value.ToString("O"))}");
        if (to.HasValue)
            parts.Add($"recordedToUtc={Uri.EscapeDataString(to.Value.ToString("O"))}");
        return "?" + string.Join("&", parts);
    }

    private static string BuildEmergencyQuery(int pageNumber, int pageSize, DateTime? from, DateTime? to)
    {
        var parts = new List<string>
        {
            $"pageNumber={pageNumber}",
            $"pageSize={pageSize}"
        };
        if (from.HasValue)
            parts.Add($"occurredFromUtc={Uri.EscapeDataString(from.Value.ToString("O"))}");
        if (to.HasValue)
            parts.Add($"occurredToUtc={Uri.EscapeDataString(to.Value.ToString("O"))}");
        return "?" + string.Join("&", parts);
    }

    private static PatientDeviceReadingListItemViewModel MapReading(DeviceReadingDto d) =>
        new()
        {
            Id = d.Id,
            DeviceId = d.DeviceId,
            SerialNumber = d.SerialNumber ?? string.Empty,
            Model = d.Model ?? string.Empty,
            Kind = d.Kind ?? string.Empty,
            ReadingType = d.ReadingType ?? string.Empty,
            PrimaryValue = d.PrimaryValue,
            Unit = d.Unit ?? string.Empty,
            RecordedAt = d.RecordedAt,
            ReceivedAt = d.ReceivedAt,
            HeartRateBpm = d.HeartRateBpm,
            SpO2Percent = d.SpO2Percent.HasValue ? (decimal)d.SpO2Percent.Value : null,
            PulseRateBpm = d.PulseRateBpm
        };

    private static DeviceReadingDailyRollupViewModel MapRollup(DailyRollupDto d)
    {
        var day = DateOnly.FromDateTime(d.Date);
        return new DeviceReadingDailyRollupViewModel
        {
            Date = day,
            HeartRateSampleCount = d.HeartRateSampleCount,
            AvgHeartRateBpm = d.AvgHeartRateBpm.HasValue
                ? decimal.Round((decimal)d.AvgHeartRateBpm.Value, 1, MidpointRounding.AwayFromZero)
                : null,
            MinHeartRateBpm = d.MinHeartRateBpm,
            MaxHeartRateBpm = d.MaxHeartRateBpm,
            SpO2SampleCount = d.SpO2SampleCount,
            AvgSpO2Percent = d.AvgSpO2Percent.HasValue
                ? decimal.Round((decimal)d.AvgSpO2Percent.Value, 1, MidpointRounding.AwayFromZero)
                : null,
            MinSpO2Percent = d.MinSpO2Percent.HasValue ? (decimal)d.MinSpO2Percent.Value : null,
            MaxSpO2Percent = d.MaxSpO2Percent.HasValue ? (decimal)d.MaxSpO2Percent.Value : null
        };
    }

    private static PatientDeviceEmergencyEventViewModel MapEmergency(EmergencyEventDto d) =>
        new()
        {
            Id = d.Id,
            DeviceId = d.DeviceId,
            SerialNumber = d.SerialNumber ?? string.Empty,
            Model = d.Model ?? string.Empty,
            EventType = d.EventType ?? string.Empty,
            OccurredAtUtc = d.OccurredAtUtc,
            ReceivedAtUtc = d.ReceivedAtUtc,
            Latitude = d.Latitude,
            Longitude = d.Longitude,
            HorizontalAccuracyMeters = d.HorizontalAccuracyMeters,
            ExternalCorrelationId = d.ExternalCorrelationId,
            CaregiversNotified = d.CaregiversNotified,
            CaregiversNotifiedAtUtc = d.CaregiversNotifiedAtUtc,
            CaregiverNotificationSummary = d.CaregiverNotificationSummary
        };

    private sealed class DeviceReadingDto
    {
        public Guid Id { get; set; }
        public Guid DeviceId { get; set; }
        public string? SerialNumber { get; set; }
        public string? Model { get; set; }
        public string? Kind { get; set; }
        public string? ReadingType { get; set; }
        public double PrimaryValue { get; set; }
        public string? Unit { get; set; }
        public DateTime RecordedAt { get; set; }
        public DateTime ReceivedAt { get; set; }
        public int? HeartRateBpm { get; set; }
        public double? SpO2Percent { get; set; }
        public int? PulseRateBpm { get; set; }
    }

    private sealed class DailyRollupDto
    {
        public DateTime Date { get; set; }
        public int HeartRateSampleCount { get; set; }
        public double? AvgHeartRateBpm { get; set; }
        public int? MinHeartRateBpm { get; set; }
        public int? MaxHeartRateBpm { get; set; }
        public int SpO2SampleCount { get; set; }
        public double? AvgSpO2Percent { get; set; }
        public double? MinSpO2Percent { get; set; }
        public double? MaxSpO2Percent { get; set; }
    }

    private sealed class EmergencyEventDto
    {
        public Guid Id { get; set; }
        public Guid DeviceId { get; set; }
        public string? SerialNumber { get; set; }
        public string? Model { get; set; }
        public string? EventType { get; set; }
        public DateTime OccurredAtUtc { get; set; }
        public DateTime ReceivedAtUtc { get; set; }
        public double? Latitude { get; set; }
        public double? Longitude { get; set; }
        public double? HorizontalAccuracyMeters { get; set; }
        public string? ExternalCorrelationId { get; set; }
        public bool CaregiversNotified { get; set; }
        public DateTime? CaregiversNotifiedAtUtc { get; set; }
        public string? CaregiverNotificationSummary { get; set; }
    }
}
