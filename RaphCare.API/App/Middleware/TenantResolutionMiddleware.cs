namespace RaphCare.API.App.Middleware;

/// <summary>
/// Extracts and validates X-Clinic-Id header and stores ClinicId in HttpContext.Items.
/// Rejects the request if the header is missing or not a valid Guid.
/// </summary>
public class TenantResolutionMiddleware(RequestDelegate next, ILogger<TenantResolutionMiddleware> logger)
{
    public const string ClinicIdItemKey = "ClinicId";
    private const string ClinicIdHeaderName = "X-Clinic-Id";

    private readonly RequestDelegate _next = next;
    private readonly ILogger<TenantResolutionMiddleware> _logger = logger;

    /// <summary>Validates X-Clinic-Id header, stores ClinicId in HttpContext.Items, or returns 400 if missing/invalid. Skips non-API and Swagger paths.</summary>
    /// <param name="context">The HTTP context.</param>
    public async Task InvokeAsync(HttpContext context)
    {
        var path = context.Request.Path.Value ?? "";
        if (path.StartsWith("/swagger", StringComparison.OrdinalIgnoreCase) || !path.StartsWith("/api/", StringComparison.OrdinalIgnoreCase))
        {
            await _next(context).ConfigureAwait(false);
            return;
        }

        if (!context.Request.Headers.TryGetValue(ClinicIdHeaderName, out var headerValue)
            || string.IsNullOrWhiteSpace(headerValue))
        {
            _logger.LogWarning("Request rejected: missing or empty {Header} header", ClinicIdHeaderName);
            context.Response.StatusCode = StatusCodes.Status400BadRequest;
            await context.Response.WriteAsJsonAsync(new { error = $"{ClinicIdHeaderName} header is required." }).ConfigureAwait(false);
            return;
        }

        if (!Guid.TryParse(headerValue.ToString().Trim(), out var clinicId))
        {
            _logger.LogWarning("Request rejected: invalid Guid in {Header}", ClinicIdHeaderName);
            context.Response.StatusCode = StatusCodes.Status400BadRequest;
            await context.Response.WriteAsJsonAsync(new { error = $"{ClinicIdHeaderName} must be a valid Guid." }).ConfigureAwait(false);
            return;
        }

        context.Items[ClinicIdItemKey] = clinicId;
        await _next(context).ConfigureAwait(false);
    }
}
