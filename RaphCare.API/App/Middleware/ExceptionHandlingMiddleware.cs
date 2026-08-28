using System.Net;
using System.Text.Json;
using Microsoft.AspNetCore.Mvc;
using RaphCare.Application.Common.Exceptions;

namespace RaphCare.API.App.Middleware;

/// <summary>
/// Catches unhandled exceptions, logs them, and returns a ProblemDetails response.
/// </summary>
public partial class ExceptionHandlingMiddleware(RequestDelegate next, ILogger<ExceptionHandlingMiddleware> logger)
{
    private static readonly JsonSerializerOptions ProblemJsonOptions = new()
    {
        PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
    };

    private readonly RequestDelegate _next = next;

    /// <summary>Invokes the next middleware; on exception, logs and returns ProblemDetails (404/400/403/500).</summary>
    /// <param name="context">The HTTP context.</param>
    public async Task InvokeAsync(HttpContext context)
    {
        try
        {
            await _next(context).ConfigureAwait(false);
        }
        catch (Exception ex)
        {
            await HandleExceptionAsync(context, ex).ConfigureAwait(false);
        }
    }

    private async Task HandleExceptionAsync(HttpContext context, Exception exception)
    {
        LogUnhandledException(exception, exception.Message);

        var (statusCode, title, detail) = exception switch
        {
            NotFoundException => (HttpStatusCode.NotFound, "Not Found", exception.Message),
            ValidationException validation => (HttpStatusCode.BadRequest, "Validation Error", "One or more validation failures occurred."),
            BusinessRuleException business => (HttpStatusCode.BadRequest, "Business Rule Violation", business.Message),
            ForbiddenAccessException => (HttpStatusCode.Forbidden, "Forbidden", "Access denied."),
            _ => (HttpStatusCode.InternalServerError, "Internal Server Error", "An unexpected error occurred.")
        };

        var problemDetails = new ProblemDetails
        {
            Type = $"https://httpstatuses.com/{(int)statusCode}",
            Title = title,
            Status = (int)statusCode,
            Detail = detail,
            Instance = context.Request.Path
        };

        if (exception is ValidationException validationEx)
        {
            problemDetails.Extensions["errors"] = validationEx.Errors;
        }

        context.Response.StatusCode = (int)statusCode;
        context.Response.ContentType = "application/problem+json";

        await context.Response.WriteAsync(JsonSerializer.Serialize(problemDetails, ProblemJsonOptions)).ConfigureAwait(false);
    }

    [LoggerMessage(Level = LogLevel.Error, Message = "Unhandled exception: {Message}")]
    private partial void LogUnhandledException(Exception exception, string message);
}
