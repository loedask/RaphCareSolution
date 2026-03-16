using Microsoft.AspNetCore.Http;
using RaphCare.Application.Common.Interfaces;

namespace RaphCare.Infrastructure.Services;

/// <summary>
/// Provides the current user from the HTTP request's authenticated principal (e.g. JWT claims).
/// </summary>
public class CurrentUserService : ICurrentUserService
{
    private readonly IHttpContextAccessor _httpContextAccessor;

    public CurrentUserService(IHttpContextAccessor httpContextAccessor)
    {
        _httpContextAccessor = httpContextAccessor ?? throw new ArgumentNullException(nameof(httpContextAccessor));
    }

    /// <inheritdoc />
    public string? UserId => _httpContextAccessor.HttpContext?.User?.FindFirst("oid")?.Value
        ?? _httpContextAccessor.HttpContext?.User?.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)?.Value
        ?? _httpContextAccessor.HttpContext?.User?.FindFirst("sub")?.Value;

    /// <inheritdoc />
    public string? UserName => _httpContextAccessor.HttpContext?.User?.Identity?.Name
        ?? _httpContextAccessor.HttpContext?.User?.FindFirst("name")?.Value
        ?? _httpContextAccessor.HttpContext?.User?.FindFirst("preferred_username")?.Value;

    /// <inheritdoc />
    public bool IsAuthenticated => _httpContextAccessor.HttpContext?.User?.Identity?.IsAuthenticated ?? false;
}
