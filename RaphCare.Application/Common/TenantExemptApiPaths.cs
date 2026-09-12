namespace RaphCare.Application.Common;

/// <summary>
/// API paths that operate above a single hospital tenant (no <c>X-Clinic-Id</c> required).
/// </summary>
public static class TenantExemptApiPaths
{
    /// <summary>True when the request path should skip clinic-header validation.</summary>
    public static bool IsExempt(string path) =>
        path.StartsWith("/api/auth/email", StringComparison.OrdinalIgnoreCase)
        || path.StartsWith("/api/admin/", StringComparison.OrdinalIgnoreCase)
        || path.StartsWith("/api/display/", StringComparison.OrdinalIgnoreCase)
        || path.StartsWith("/api/webhooks/", StringComparison.OrdinalIgnoreCase)
        || path.StartsWith("/api/devices", StringComparison.OrdinalIgnoreCase)
        || path.StartsWith("/api/ops", StringComparison.OrdinalIgnoreCase)
        // Patient membership / care-history list. Used before Active clinic is set on the phone.
        || path.StartsWith("/api/patient/clinics", StringComparison.OrdinalIgnoreCase);
}
