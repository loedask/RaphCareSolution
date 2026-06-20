using MediatR;
using Microsoft.Extensions.Logging;

namespace RaphCare.Application.Common.Behaviors;

/// <summary>
/// MediatR pipeline behavior that logs the start and completion of each request for observability.
/// </summary>
public class LoggingBehavior<TRequest, TResponse> : IPipelineBehavior<TRequest, TResponse>
    where TRequest : notnull
{
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
        _logger.LogInformation("Handling request {RequestName}", typeof(TRequest).Name);
        var response = await next(cancellationToken);
        _logger.LogInformation("Handled request {RequestName}", typeof(TRequest).Name);
        return response;
    }
}

