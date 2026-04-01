using RaphCare.Client.Contracts;
using RaphCare.Client.Contracts.Interfaces;
using RaphCare.Client.Models.Devices;
using RaphCare.Client.Services.Base;

namespace RaphCare.Client.Services;

/// <summary>Wraps generated <see cref="IClient"/> patient device operations (<c>api/patient/devices</c>).</summary>
public sealed class PatientDevicesService(IClient client) : IPatientDevicesService
{
    public async Task<Response<IReadOnlyList<PatientDeviceListItemViewModel>>> GetMyDevicesAsync(CancellationToken cancellationToken = default)
    {
        try
        {
            var dtos = await client.GetMyDevicesAsync(cancellationToken).ConfigureAwait(false);
            var list = (dtos ?? Array.Empty<PatientDeviceListItemDto>())
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
        catch (global::RaphCare.Client.Services.Base.ApiException ex)
        {
            return Response<IReadOnlyList<PatientDeviceListItemViewModel>>.Failure(ex.Message, ex.StatusCode);
        }
    }

    public async Task<Response<RegisterMyDeviceResultViewModel>> RegisterMyDeviceAsync(
        string serialNumber,
        string modelSku,
        CancellationToken cancellationToken = default)
    {
        try
        {
            var body = new RegisterMyDeviceCommand
            {
                SerialNumber = serialNumber,
                ModelSku = modelSku
            };
            var dto = await client.RegisterMyDeviceAsync(body, cancellationToken).ConfigureAwait(false);
            return Response<RegisterMyDeviceResultViewModel>.Success(
                new RegisterMyDeviceResultViewModel { DeviceId = dto.DeviceId });
        }
        catch (global::RaphCare.Client.Services.Base.ApiException ex)
        {
            return Response<RegisterMyDeviceResultViewModel>.Failure(ex.Message, ex.StatusCode);
        }
    }

    public async Task<Response<SyncMyDeviceReadingsResultViewModel>> SyncReadingsAsync(
        Guid deviceId,
        IReadOnlyList<HeartRateReadingInput> heartRates,
        IReadOnlyList<Spo2ReadingInput> spo2,
        CancellationToken cancellationToken = default)
    {
        try
        {
            var body = new SyncMyDeviceReadingsCommand
            {
                DeviceId = deviceId,
                HeartRates = (heartRates ?? Array.Empty<HeartRateReadingInput>())
                    .Select(h => new HeartRatePointDto { RecordedAt = h.RecordedAt, BeatsPerMinute = h.BeatsPerMinute })
                    .ToList(),
                Spo2 = (spo2 ?? Array.Empty<Spo2ReadingInput>())
                    .Select(s => new Spo2PointDto
                    {
                        RecordedAt = s.RecordedAt,
                        SpO2 = (double)s.SpO2,
                        PulseRate = s.PulseRate.HasValue ? (double)s.PulseRate.Value : (double?)null
                    })
                    .ToList()
            };

            var dto = await client.SyncMyDeviceReadingsAsync(deviceId, body, cancellationToken).ConfigureAwait(false);
            return Response<SyncMyDeviceReadingsResultViewModel>.Success(
                new SyncMyDeviceReadingsResultViewModel
                {
                    HeartRateCount = dto.HeartRateCount,
                    SpO2Count = dto.SpO2Count
                });
        }
        catch (global::RaphCare.Client.Services.Base.ApiException ex)
        {
            return Response<SyncMyDeviceReadingsResultViewModel>.Failure(ex.Message, ex.StatusCode);
        }
    }
}
