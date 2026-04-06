using RaphCare.Client.Contracts.Interfaces;
using RaphCare.Client.Models.Devices;

namespace RaphCare.Mobile.Core.Features.Devices.Services;

/// <summary>
/// Persists vitals sync payloads when the API is unreachable so they can be retried later from the Devices screen.
/// </summary>
public interface IVitalsSyncOutbox
{
    /// <summary>Queues a copy of the readings for the given provisioned device.</summary>
    Task EnqueueAsync(
        Guid deviceId,
        IReadOnlyList<HeartRateReadingInput> heartRates,
        IReadOnlyList<Spo2ReadingInput> spo2Readings,
        CancellationToken cancellationToken = default);

    /// <summary>Attempts each pending batch via <paramref name="patientDevices"/>; removes successful uploads.</summary>
    /// <returns>Number of batches successfully synced.</returns>
    Task<int> TryFlushAsync(IPatientDevicesService patientDevices, CancellationToken cancellationToken = default);
}
