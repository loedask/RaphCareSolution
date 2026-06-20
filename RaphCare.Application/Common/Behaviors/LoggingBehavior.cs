using MediatR;
using Microsoft.Extensions.Logging;

namespace RaphCare.Application.Common.Behaviors;

/// <summary>
/// MediatR pipeline behavior that logs the start and completion of each request for observability.
/// </summary>
public class LoggingBehavior<TRequest, TResponse> : IPipelineBehavior<TRequest, TResponse>
    where TRequest : notnull
{
    private static readonly Action<ILogger, string, Exception?> LogHandling =
        LoggerMessage.Define<string>(
            LogLevel.Information,
            new EventId(1, nameof(LogHandling)),
            "Handling request {RequestName}");

    private static readonly Action<ILogger, string, Exception?> LogHandled =
        LoggerMessage.Define<string>(
            LogLevel.Information,
            new EventId(2, nameof(LogHandled)),
            "Handled request {RequestName}");

    private readonly ILogger<LoggingBehavior<TRequest, TResponse>> _logger;

    /// <summary>Creates the logging behavior.</summary>
    public LoggingBehavior(ILogger<LoggingBehavior<TRequest, TResponse>> logger)
    {
        _logger = logger;
    }

    /// <inheritdoc />
    public async Task<TResponse> Handle(
        TRequest request,
        RequestHandlerDelegate<TResponse> next,
        CancellationToken cancellationToken)
    {
        var requestName = typeof(TRequest).Name;
        LogHandling(_logger, requestName, null);
        var response = await next(cancellationToken);
        LogHandled(_logger, requestName, null);
        return response;
    }
}
