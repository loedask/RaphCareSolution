using Microsoft.Extensions.Logging;

namespace RaphCare.Infrastructure.Services;

/// <summary>Abstraction for ingesting and syncing data from external health devices.</summary>
public interface IDeviceIntegrationService
{
    Task IngestDeviceReadingAsync(Guid deviceId, Guid patientId, string readingType, decimal value, DateTime recordedAt, CancellationToken cancellationToken = default);
    Task SyncDeviceAsync(Guid deviceId, CancellationToken cancellationToken = default);
}

/// <summary>Device integration. Placeholder for external device SDK or ingestion pipeline.</summary>
public class DeviceIntegrationService : IDeviceIntegrationService
{
    private readonly ILogger<DeviceIntegrationService> _logger;

    public DeviceIntegrationService(ILogger<DeviceIntegrationService> logger)
    {
        _logger = logger;
    }

    public Task IngestDeviceReadingAsync(Guid deviceId, Guid patientId, string readingType, decimal value, DateTime recordedAt, CancellationToken cancellationToken = default)
    {
        _logger.LogInformation("Device ingest placeholder: DeviceId={DeviceId}, Type={Type}, Value={Value}", deviceId, readingType, value);
        // TODO: Integrate with device SDK / ingestion pipeline
        return Task.CompletedTask;
    }

    public Task SyncDeviceAsync(Guid deviceId, CancellationToken cancellationToken = default)
    {
        _logger.LogInformation("Device sync placeholder: DeviceId={DeviceId}", deviceId);
        return Task.CompletedTask;
    }
}
