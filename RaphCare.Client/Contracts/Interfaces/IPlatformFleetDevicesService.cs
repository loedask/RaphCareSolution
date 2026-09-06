using RaphCare.Client.Contracts;
using RaphCare.Client.Models.Fleet;

namespace RaphCare.Client.Contracts.Interfaces;

/// <summary>Platform fleet wearable inventory (<c>api/Devices</c>, platform admin).</summary>
public interface IPlatformFleetDevicesService
{
    Task<Response<PagedFleetDevices>> GetDevicesAsync(
        int pageNumber = 1,
        int pageSize = 20,
        Guid? clinicId = null,
        bool? unassignedOnly = null,
        string? serialContains = null,
        CancellationToken cancellationToken = default);

    Task<Response<Guid>> CreateDeviceAsync(
        Guid clinicId,
        string serialNumber,
        string model,
        string? bluetoothMacAddress = null,
        CancellationToken cancellationToken = default);

    Task<Response<Guid>> AssignDeviceToPatientAsync(
        Guid deviceId,
        Guid patientId,
        CancellationToken cancellationToken = default);

    Task<Response<Guid>> UnassignDeviceFromPatientAsync(
        Guid deviceId,
        CancellationToken cancellationToken = default);

    Task<Response<bool>> DeleteDeviceAsync(
        Guid deviceId,
        CancellationToken cancellationToken = default);
}
