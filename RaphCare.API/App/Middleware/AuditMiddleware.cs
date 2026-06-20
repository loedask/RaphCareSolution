using System.Security.Claims;

namespace RaphCare.API.App.Middleware;

/// <summary>
/// Logs request path, user id (if authenticated), and timestamp for audit trail.
/// </summary>
public partial class AuditMiddleware(RequestDelegate next, ILogger<AuditMiddleware> logger)
{
    private readonly RequestDelegate _next = next;

    /// <summary>Logs request path, user id, and timestamp then invokes the next middleware.</summary>
    /// <param name="context">The HTTP context.</param>
    public async Task InvokeAsync(HttpContext context)
    {
        var path = context.Request.Path.Value ?? "-";
        var userId = context.User.FindFirstValue(ClaimTypes.NameIdentifier)
            ?? context.User.FindFirstValue("sub")
            ?? "anonymous";
        var timestamp = DateTime.UtcNow;

        LogAudit(path, userId, timestamp);

        await _next(context).ConfigureAwait(false);
    }

    [LoggerMessage(Level = LogLevel.Information, Message = "Audit: Path={Path}, UserId={UserId}, Timestamp={Timestamp:O}")]
    private partial void LogAudit(string path, string userId, DateTime timestamp);
}
