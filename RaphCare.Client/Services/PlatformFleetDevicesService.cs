using System.Text;
using RaphCare.Client.Contracts;
using RaphCare.Client.Contracts.Interfaces;
using RaphCare.Client.Models.Api;
using RaphCare.Client.Models.Fleet;
using RaphCare.Client.Services.Base;

namespace RaphCare.Client.Services;

public sealed class PlatformFleetDevicesService(HttpClient httpClient) : BaseHttpService(httpClient), IPlatformFleetDevicesService
{
    public async Task<Response<PagedFleetDevices>> GetDevicesAsync(
        int pageNumber = 1,
        int pageSize = 20,
        Guid? clinicId = null,
        bool? unassignedOnly = null,
        string? serialContains = null,
        CancellationToken cancellationToken = default)
    {
        var qs = new StringBuilder($"api/Devices?pageNumber={pageNumber}&pageSize={pageSize}");
        if (clinicId is Guid c)
            qs.Append("&clinicId=").Append(Uri.EscapeDataString(c.ToString()));
        if (unassignedOnly is bool u)
            qs.Append("&unassignedOnly=").Append(u ? "true" : "false");
        if (!string.IsNullOrWhiteSpace(serialContains))
            qs.Append("&serialContains=").Append(Uri.EscapeDataString(serialContains.Trim()));

        var result = await GetAsync<PagedApiResult<FleetDeviceDto>>(qs.ToString(), cancellationToken)
            .ConfigureAwait(false);
        if (!result.IsSuccess || result.Data is null)
            return Response<PagedFleetDevices>.Failure(result.ErrorMessage ?? "Could not load fleet devices.", result.StatusCode);

        var page = result.Data;
        return Response<PagedFleetDevices>.Success(new PagedFleetDevices
        {
            Items = (page.Items ?? Array.Empty<FleetDeviceDto>())
                .Select(d => new FleetDeviceListItem
                {
                    Id = d.Id,
                    ClinicId = d.ClinicId,
                    SerialNumber = d.SerialNumber ?? string.Empty,
                    Model = d.Model ?? string.Empty,
                    BluetoothMacAddress = d.BluetoothMacAddress,
                    ActivatedAt = d.ActivatedAt,
                    IsActive = d.IsActive,
                    IsAssigned = d.IsAssigned,
                    Status = d.Status ?? string.Empty
                })
                .ToList(),
            TotalCount = page.TotalCount,
            PageNumber = page.PageNumber,
            PageSize = page.PageSize
        });
    }

    public async Task<Response<Guid>> CreateDeviceAsync(
        Guid clinicId,
        string serialNumber,
        string model,
        string? bluetoothMacAddress = null,
        CancellationToken cancellationToken = default)
    {
        var result = await PostAsync<CreatedIdDto>(
                "api/Devices",
                new
                {
                    clinicId,
                    serialNumber,
                    model,
                    bluetoothMacAddress,
                    deviceTypeId = Guid.Empty,
                    deviceManufacturerId = Guid.Empty
                },
                cancellationToken)
            .ConfigureAwait(false);

        if (!result.IsSuccess || result.Data is null || result.Data.Id == Guid.Empty)
            return Response<Guid>.Failure(result.ErrorMessage ?? "Could not add device to fleet.", result.StatusCode);

        return Response<Guid>.Success(result.Data.Id);
    }

    public async Task<Response<Guid>> AssignDeviceToPatientAsync(
        Guid deviceId,
        Guid patientId,
        CancellationToken cancellationToken = default)
    {
        var result = await PostAsync<CreatedIdDto>(
                $"api/Devices/{deviceId}/assign",
                new { patientId },
                cancellationToken)
            .ConfigureAwait(false);

        if (!result.IsSuccess || result.Data is null || result.Data.Id == Guid.Empty)
            return Response<Guid>.Failure(result.ErrorMessage ?? "Could not assign device.", result.StatusCode);

        return Response<Guid>.Success(result.Data.Id);
    }

    public async Task<Response<Guid>> UnassignDeviceFromPatientAsync(
        Guid deviceId,
        CancellationToken cancellationToken = default)
    {
        var result = await PostAsync<CreatedIdDto>(
                $"api/Devices/{deviceId}/unassign",
                new { },
                cancellationToken)
            .ConfigureAwait(false);

        if (!result.IsSuccess || result.Data is null || result.Data.Id == Guid.Empty)
            return Response<Guid>.Failure(result.ErrorMessage ?? "Could not revoke assignment.", result.StatusCode);

        return Response<Guid>.Success(result.Data.Id);
    }

    public async Task<Response<bool>> DeleteDeviceAsync(
        Guid deviceId,
        CancellationToken cancellationToken = default)
    {
        var result = await DeleteAsync($"api/Devices/{deviceId}", cancellationToken).ConfigureAwait(false);
        return result.IsSuccess
            ? Response<bool>.Success(true)
            : Response<bool>.Failure(result.ErrorMessage ?? "Could not delete device.", result.StatusCode);
    }

    public async Task<Response<bool>> SetDeviceBluetoothMacAsync(
        Guid deviceId,
        string bluetoothMacAddress,
        CancellationToken cancellationToken = default)
    {
        var result = await PutNoContentAsync(
                $"api/Devices/{deviceId}/bluetooth-mac",
                new { bluetoothMacAddress },
                cancellationToken)
            .ConfigureAwait(false);
        return result.IsSuccess
            ? Response<bool>.Success(true)
            : Response<bool>.Failure(result.ErrorMessage ?? "Could not save Bluetooth MAC.", result.StatusCode);
    }

    private sealed class FleetDeviceDto
    {
        public Guid Id { get; set; }
        public Guid ClinicId { get; set; }
        public string? SerialNumber { get; set; }
        public string? Model { get; set; }
        public string? BluetoothMacAddress { get; set; }
        public DateTime? ActivatedAt { get; set; }
        public bool IsActive { get; set; }
        public bool IsAssigned { get; set; }
        public string? Status { get; set; }
    }

    private sealed class CreatedIdDto
    {
        public Guid Id { get; set; }
    }
}
