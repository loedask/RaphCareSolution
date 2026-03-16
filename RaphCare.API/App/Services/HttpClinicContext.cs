using Microsoft.AspNetCore.Http;
using RaphCare.API.App.Middleware;
using RaphCare.Application.Common.Interfaces;

namespace RaphCare.API.App.Services;

/// <summary>
/// Clinic context backed by HttpContext.Items, populated by TenantResolutionMiddleware.
/// </summary>
public class HttpClinicContext(IHttpContextAccessor httpContextAccessor) : IClinicContext
{
    public Guid? ClinicId
    {
        get
        {
            var httpContext = httpContextAccessor.HttpContext;
            if (httpContext is null) return null;

            return httpContext.Items.TryGetValue(TenantResolutionMiddleware.ClinicIdItemKey, out var value)
                && value is Guid clinicId
                ? clinicId
                : null;
        }
    }
}

