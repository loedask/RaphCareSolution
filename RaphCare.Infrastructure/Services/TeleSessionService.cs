using Microsoft.Extensions.Logging;

namespace RaphCare.Infrastructure.Services;

/// <summary>Abstraction for creating and ending telemedicine sessions (e.g. video calls).</summary>
public interface ITeleSessionService
{
    Task<Guid> CreateSessionAsync(Guid appointmentId, Guid visitId, string platform, CancellationToken cancellationToken = default);
    Task EndSessionAsync(Guid sessionId, CancellationToken cancellationToken = default);
}

/// <summary>Telemedicine session integration. Placeholder for Azure Communication Services or WebRTC.</summary>
public sealed partial class TeleSessionService(ILogger<TeleSessionService> logger) : ITeleSessionService
{
    public Task<Guid> CreateSessionAsync(Guid appointmentId, Guid visitId, string platform, CancellationToken cancellationToken = default)
    {
        LogTeleSessionCreatePlaceholder(appointmentId, platform);
        return Task.FromResult(Guid.NewGuid());
    }

    public Task EndSessionAsync(Guid sessionId, CancellationToken cancellationToken = default)
    {
        LogTeleSessionEndPlaceholder(sessionId);
        return Task.CompletedTask;
    }

    [LoggerMessage(
        Level = LogLevel.Information,
        Message = "Tele session create placeholder: AppointmentId={AppointmentId}, Platform={Platform}")]
    private partial void LogTeleSessionCreatePlaceholder(Guid appointmentId, string platform);

    [LoggerMessage(Level = LogLevel.Information, Message = "Tele session end placeholder: SessionId={SessionId}")]
    private partial void LogTeleSessionEndPlaceholder(Guid sessionId);
}
