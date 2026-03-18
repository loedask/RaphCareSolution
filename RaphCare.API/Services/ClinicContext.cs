using Microsoft.AspNetCore.Http;
using RaphCare.API.App.Middleware;
using RaphCare.Application.Common.Interfaces;

namespace RaphCare.API.Services;

/// <summary>
/// Clinic context backed by <see cref="HttpContext.Items"/> populated by <see cref="TenantResolutionMiddleware"/>.
/// </summary>
public class ClinicContext(IHttpContextAccessor httpContextAccessor) : IClinicContext
{
    private readonly IHttpContextAccessor _httpContextAccessor = httpContextAccessor ?? throw new ArgumentNullException(nameof(httpContextAccessor));

    /// <inheritdoc />
    public Guid? ClinicId
    {
        get
        {
            var httpContext = _httpContextAccessor.HttpContext;
            if (httpContext is null)
                return null;

            return httpContext.Items.TryGetValue(TenantResolutionMiddleware.ClinicIdItemKey, out var value)
                && value is Guid clinicId
                    ? clinicId
                    : null;
        }
    }
}

