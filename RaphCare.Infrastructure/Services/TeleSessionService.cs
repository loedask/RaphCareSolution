using Microsoft.Extensions.Logging;

namespace RaphCare.Infrastructure.Services;

/// <summary>Abstraction for creating and ending telemedicine sessions (e.g. video calls).</summary>
public interface ITeleSessionService
{
    Task<Guid> CreateSessionAsync(Guid appointmentId, Guid visitId, string platform, CancellationToken cancellationToken = default);
    Task EndSessionAsync(Guid sessionId, CancellationToken cancellationToken = default);
}

/// <summary>Telemedicine session integration. Placeholder for Azure Communication Services or WebRTC.</summary>
public class TeleSessionService : ITeleSessionService
{
    private readonly ILogger<TeleSessionService> _logger;

    public TeleSessionService(ILogger<TeleSessionService> logger)
    {
        _logger = logger;
    }

    public Task<Guid> CreateSessionAsync(Guid appointmentId, Guid visitId, string platform, CancellationToken cancellationToken = default)
    {
        _logger.LogInformation("Tele session create placeholder: AppointmentId={AppointmentId}, Platform={Platform}", appointmentId, platform);
        // TODO: Integrate with Azure Communication Services or WebRTC
        return Task.FromResult(Guid.NewGuid());
    }

    public Task EndSessionAsync(Guid sessionId, CancellationToken cancellationToken = default)
    {
        _logger.LogInformation("Tele session end placeholder: SessionId={SessionId}", sessionId);
        return Task.CompletedTask;
    }
}
