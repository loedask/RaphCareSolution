using Microsoft.Extensions.Logging;

namespace RaphCare.Infrastructure.Services;

/// <summary>Abstraction for ingesting and syncing data from external health devices.</summary>
public interface IDeviceIntegrationService
{
    Task IngestDeviceReadingAsync(Guid deviceId, Guid patientId, string readingType, decimal value, DateTime recordedAt, CancellationToken cancellationToken = default);
    Task SyncDeviceAsync(Guid deviceId, CancellationToken cancellationToken = default);
}

/// <summary>Device integration. Placeholder for external device SDK or ingestion pipeline.</summary>
public sealed partial class DeviceIntegrationService(ILogger<DeviceIntegrationService> logger) : IDeviceIntegrationService
{
    public Task IngestDeviceReadingAsync(Guid deviceId, Guid patientId, string readingType, decimal value, DateTime recordedAt, CancellationToken cancellationToken = default)
    {
        LogDeviceIngestPlaceholder(deviceId, readingType, value);
        return Task.CompletedTask;
    }

    public Task SyncDeviceAsync(Guid deviceId, CancellationToken cancellationToken = default)
    {
        LogDeviceSyncPlaceholder(deviceId);
        return Task.CompletedTask;
    }

    [LoggerMessage(
        Level = LogLevel.Information,
        Message = "Device ingest placeholder: DeviceId={DeviceId}, Type={Type}, Value={Value}")]
    private partial void LogDeviceIngestPlaceholder(Guid deviceId, string type, decimal value);

    [LoggerMessage(Level = LogLevel.Information, Message = "Device sync placeholder: DeviceId={DeviceId}")]
    private partial void LogDeviceSyncPlaceholder(Guid deviceId);
}
