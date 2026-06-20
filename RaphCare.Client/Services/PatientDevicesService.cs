using RaphCare.Client.Contracts;
using RaphCare.Client.Contracts.Interfaces;
using RaphCare.Client.Models.Appointments;
using RaphCare.Client.Models.Devices;
using RaphCare.Client.Services.Base;

namespace RaphCare.Client.Services;

public sealed class PatientDevicesService(HttpClient httpClient) : BaseHttpService(httpClient), IPatientDevicesService
{
    public async Task<Response<IReadOnlyList<PatientDeviceListItemViewModel>>> GetMyDevicesAsync(CancellationToken cancellationToken = default)
    {
        var result = await GetAsync<IReadOnlyList<DeviceListItemDto>>("api/patient/devices", cancellationToken).ConfigureAwait(false);
        if (!result.IsSuccess)
            return Response<IReadOnlyList<PatientDeviceListItemViewModel>>.Failure(result.ErrorMessage ?? "Could not load devices.", result.StatusCode);

        var list = (result.Data ?? Array.Empty<DeviceListItemDto>())
            .Select(d => new PatientDeviceListItemViewModel
            {
                DeviceId = d.DeviceId,
                SerialNumber = d.SerialNumber ?? string.Empty,
                Model = d.Model ?? string.Empty,
                AssignedAt = d.AssignedAt
            })
            .ToList();
        return Response<IReadOnlyList<PatientDeviceListItemViewModel>>.Success(list);
    }

    public async Task<Response<RegisterMyDeviceResultViewModel>> RegisterMyDeviceAsync(
        string serialNumber,
        string modelSku,
        CancellationToken cancellationToken = default)
    {
        var result = await PostAsync<RegisterDeviceResponseDto>(
                "api/patient/devices/register",
                new { serialNumber, modelSku },
                cancellationToken)
            .ConfigureAwait(false);

        if (!result.IsSuccess || result.Data is null)
            return Response<RegisterMyDeviceResultViewModel>.Failure(result.ErrorMessage ?? "Register failed.", result.StatusCode);

        return Response<RegisterMyDeviceResultViewModel>.Success(
            new RegisterMyDeviceResultViewModel { DeviceId = result.Data.DeviceId });
    }

    public async Task<Response<SyncMyDeviceReadingsResultViewModel>> SyncReadingsAsync(
        Guid deviceId,
        IReadOnlyList<HeartRateReadingInput> heartRates,
        IReadOnlyList<Spo2ReadingInput> spo2,
        CancellationToken cancellationToken = default)
    {
        var body = new
        {
            deviceId,
            heartRates = (heartRates ?? Array.Empty<HeartRateReadingInput>())
                .Select(h => new { recordedAt = h.RecordedAt, beatsPerMinute = h.BeatsPerMinute })
                .ToList(),
            spo2 = (spo2 ?? Array.Empty<Spo2ReadingInput>())
                .Select(s => new
                {
                    recordedAt = s.RecordedAt,
                    spO2 = (double)s.SpO2,
                    pulseRate = s.PulseRate.HasValue ? (double?)s.PulseRate.Value : null
                })
                .ToList()
        };

        var result = await PostAsync<SyncReadingsResponseDto>(
                $"api/patient/devices/{deviceId}/readings",
                body,
                cancellationToken)
            .ConfigureAwait(false);

        if (!result.IsSuccess || result.Data is null)
            return Response<SyncMyDeviceReadingsResultViewModel>.Failure(result.ErrorMessage ?? "Sync failed.", result.StatusCode);

        return Response<SyncMyDeviceReadingsResultViewModel>.Success(
            new SyncMyDeviceReadingsResultViewModel
            {
                HeartRateCount = result.Data.HeartRateCount,
                SpO2Count = result.Data.SpO2Count
            });
    }

    private sealed class DeviceListItemDto
    {
        public Guid DeviceId { get; set; }
        public string? SerialNumber { get; set; }
        public string? Model { get; set; }
        public DateTime AssignedAt { get; set; }
    }

    private sealed class RegisterDeviceResponseDto
    {
        public Guid DeviceId { get; set; }
    }

    private sealed class SyncReadingsResponseDto
    {
        public int HeartRateCount { get; set; }
        public int SpO2Count { get; set; }
    }
}
