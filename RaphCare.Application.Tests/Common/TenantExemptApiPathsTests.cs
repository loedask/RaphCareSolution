using RaphCare.Application.Common;
using Xunit;

namespace RaphCare.Application.Tests.Common;

/// <summary>
/// Platform fleet APIs and patient linked-clinic list must not require X-Clinic-Id.
/// Ops has no Portal clinic profile; Active clinic may be unset when listing memberships.
/// </summary>
public sealed class TenantExemptApiPathsTests
{
    [Theory]
    [InlineData("/api/devices")]
    [InlineData("/api/Devices")]
    [InlineData("/api/Devices/aaaaaaaa-bbbb-cccc-dddd-eeeeeeeeeeee")]
    [InlineData("/api/ops")]
    [InlineData("/api/ops/stats")]
    [InlineData("/api/admin/clinics")]
    [InlineData("/api/auth/email/signin")]
    [InlineData("/api/display/collection/abc")]
    [InlineData("/api/patient/clinics")]
    [InlineData("/api/patient/Clinics")]
    public void FleetAndAdminPathsAreExempt(string path) =>
        Assert.True(TenantExemptApiPaths.IsExempt(path));

    [Theory]
    [InlineData("/api/patients")]
    [InlineData("/api/clinical/visits")]
    [InlineData("/api/patient/profile")]
    [InlineData("/api/patient/appointments")]
    public void TenantScopedPathsAreNotExempt(string path) =>
        Assert.False(TenantExemptApiPaths.IsExempt(path));
}
