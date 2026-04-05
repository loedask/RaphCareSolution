using RaphCare.Client.Contracts;
using RaphCare.Client.Contracts.Interfaces;
using RaphCare.Client.Models.Clinical;
using RaphCare.Client.Services.Base;

namespace RaphCare.Client.Services;

/// <summary>Wraps generated <see cref="IClient"/> clinical device-reading operations (staff token).</summary>
public sealed class ClinicalPatientDeviceReadingsService(IClient client) : IClinicalPatientDeviceReadingsService
{
    private readonly IClient _client = client;

    public async Task<Response<PagedPatientDeviceReadingsViewModel>> GetReadingsAsync(
        Guid patientId,
        int pageNumber = 1,
        int pageSize = 20,
        string? readingType = null,
        DateTime? recordedFromUtc = null,
        DateTime? recordedToUtc = null,
        CancellationToken cancellationToken = default)
    {
        try
        {
            var dto = await _client.GetPatientDeviceReadingsAsync(
                    patientId,
                    pageNumber,
                    pageSize,
                    readingType,
                    recordedFromUtc,
                    recordedToUtc,
                    cancellationToken)
                .ConfigureAwait(false);

            var items = (dto.Items ?? Array.Empty<PatientDeviceReadingListItemDto>())
                .Select(MapReading)
                .ToList();

            return Response<PagedPatientDeviceReadingsViewModel>.Success(
                new PagedPatientDeviceReadingsViewModel
                {
                    Items = items,
                    TotalCount = dto.TotalCount,
                    PageNumber = dto.PageNumber,
                    PageSize = dto.PageSize
                });
        }
        catch (global::RaphCare.Client.Services.Base.ApiException ex)
        {
            return Response<PagedPatientDeviceReadingsViewModel>.Failure(ex.Message, ex.StatusCode);
        }
    }

    public async Task<Response<IReadOnlyList<DeviceReadingDailyRollupViewModel>>> GetDailyRollupAsync(
        Guid patientId,
        DateTime fromUtc,
        DateTime toUtc,
        CancellationToken cancellationToken = default)
    {
        try
        {
            var rows = await _client
                .GetPatientDeviceReadingDailyRollupAsync(patientId, fromUtc, toUtc, cancellationToken)
                .ConfigureAwait(false);

            var list = (rows ?? Array.Empty<DeviceReadingDailyRollupDto>())
                .Select(MapRollup)
                .ToList();

            return Response<IReadOnlyList<DeviceReadingDailyRollupViewModel>>.Success(list);
        }
        catch (global::RaphCare.Client.Services.Base.ApiException ex)
        {
            return Response<IReadOnlyList<DeviceReadingDailyRollupViewModel>>.Failure(ex.Message, ex.StatusCode);
        }
    }

    public async Task<Response<PagedPatientDeviceEmergencyEventsViewModel>> GetEmergencyEventsAsync(
        Guid patientId,
        int pageNumber = 1,
        int pageSize = 20,
        DateTime? occurredFromUtc = null,
        DateTime? occurredToUtc = null,
        CancellationToken cancellationToken = default)
    {
        try
        {
            var dto = await _client.GetPatientDeviceEmergencyEventsAsync(
                    patientId,
                    pageNumber,
                    pageSize,
                    occurredFromUtc,
                    occurredToUtc,
                    cancellationToken)
                .ConfigureAwait(false);

            var items = (dto.Items ?? Array.Empty<PatientDeviceEmergencyEventListItemDto>())
                .Select(MapEmergency)
                .ToList();

            return Response<PagedPatientDeviceEmergencyEventsViewModel>.Success(
                new PagedPatientDeviceEmergencyEventsViewModel
                {
                    Items = items,
                    TotalCount = dto.TotalCount,
                    PageNumber = dto.PageNumber,
                    PageSize = dto.PageSize
                });
        }
        catch (global::RaphCare.Client.Services.Base.ApiException ex)
        {
            return Response<PagedPatientDeviceEmergencyEventsViewModel>.Failure(ex.Message, ex.StatusCode);
        }
    }

    private static PatientDeviceReadingListItemViewModel MapReading(PatientDeviceReadingListItemDto d) =>
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

    private static DeviceReadingDailyRollupViewModel MapRollup(DeviceReadingDailyRollupDto d)
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

    private static PatientDeviceEmergencyEventViewModel MapEmergency(PatientDeviceEmergencyEventListItemDto d) =>
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
}
