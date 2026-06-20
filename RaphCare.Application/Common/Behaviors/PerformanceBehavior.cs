using MediatR;
using Microsoft.Extensions.Logging;
using System.Diagnostics;

namespace RaphCare.Application.Common.Behaviors;

/// <summary>
/// MediatR pipeline behavior that measures handler execution time and logs a warning when it exceeds 500 ms.
/// </summary>
public class PerformanceBehavior<TRequest, TResponse> : IPipelineBehavior<TRequest, TResponse>
    where TRequest : notnull
{
    private static readonly Action<ILogger, string, long, Exception?> LogLongRunning =
        LoggerMessage.Define<string, long>(
            LogLevel.Warning,
            new EventId(1, nameof(LogLongRunning)),
            "Long running request {RequestName} ({ElapsedMilliseconds} ms)");

    private readonly ILogger<PerformanceBehavior<TRequest, TResponse>> _logger;
    private readonly Stopwatch _timer = new();

    /// <summary>Creates the performance behavior.</summary>
    public PerformanceBehavior(ILogger<PerformanceBehavior<TRequest, TResponse>> logger)
    {
        _logger = logger;
    }

    public async Task<TResponse> Handle(
        TRequest request,
        RequestHandlerDelegate<TResponse> next,
        CancellationToken cancellationToken)
    {
        _timer.Start();
        var response = await next(cancellationToken);
        _timer.Stop();

        if (_timer.ElapsedMilliseconds > 500)
        {
            LogLongRunning(_logger, typeof(TRequest).Name, _timer.ElapsedMilliseconds, null);
        }

        return response;
    }
}
