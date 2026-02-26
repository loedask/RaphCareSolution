using System.Security.Claims;

namespace RaphCare.API.Middleware;

/// <summary>
/// Logs request path, user id (if authenticated), and timestamp for audit trail.
/// </summary>
public class AuditMiddleware(RequestDelegate next, ILogger<AuditMiddleware> logger)
{
    private readonly RequestDelegate _next = next;
    private readonly ILogger<AuditMiddleware> _logger = logger;

    public async Task InvokeAsync(HttpContext context)
    {
        var path = context.Request.Path.Value ?? "-";
        var userId = context.User.FindFirstValue(ClaimTypes.NameIdentifier)
            ?? context.User.FindFirstValue("sub")
            ?? "anonymous";
        var timestamp = DateTime.UtcNow;

        _logger.LogInformation("Audit: Path={Path}, UserId={UserId}, Timestamp={Timestamp:O}",
            path, userId, timestamp);

        await _next(context).ConfigureAwait(false);
    }
}
