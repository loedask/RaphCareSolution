using RaphCare.Client.Contracts;
using RaphCare.Client.Models.Devices;

namespace RaphCare.Client.Contracts.Interfaces;

/// <summary>Patient wearable registry and vitals sync (<c>api/patient/devices</c>).</summary>
public interface IPatientDevicesService
{
    Task<Response<IReadOnlyList<PatientDeviceListItemViewModel>>> GetMyDevicesAsync(CancellationToken cancellationToken = default);

    Task<Response<PatientLatestReadingsViewModel>> GetMyLatestReadingsAsync(CancellationToken cancellationToken = default);

    Task<Response<RegisterMyDeviceResultViewModel>> RegisterMyDeviceAsync(string serialNumber, string modelSku, CancellationToken cancellationToken = default);

    Task<Response<SyncMyDeviceReadingsResultViewModel>> SyncReadingsAsync(
        Guid deviceId,
        IReadOnlyList<HeartRateReadingInput> heartRates,
        IReadOnlyList<Spo2ReadingInput> spo2,
        CancellationToken cancellationToken = default);
}
