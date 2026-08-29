using System.Net;
using RaphCare.Client.Services.Base;
using Xunit;

namespace RaphCare.Client.Tests.Services.Base;

public sealed class ApiErrorMessagesTests
{
    [Fact]
    public void FromBodyRewritesMissingClinicHeaderJsonToFriendlyMessage()
    {
        var message = ApiErrorMessages.FromBody(
            """{"error":"X-Clinic-Id header is required."}""",
            HttpStatusCode.BadRequest);

        Assert.DoesNotContain("X-Clinic-Id", message, StringComparison.OrdinalIgnoreCase);
        Assert.Contains("Profile", message, StringComparison.OrdinalIgnoreCase);
        Assert.Contains("clinic", message, StringComparison.OrdinalIgnoreCase);
    }

    [Fact]
    public void FromBodyRewritesInvalidClinicHeaderToFriendlyMessage()
    {
        var message = ApiErrorMessages.FromBody(
            """{"error":"X-Clinic-Id must be a valid Guid."}""",
            HttpStatusCode.BadRequest);

        Assert.DoesNotContain("valid Guid", message, StringComparison.OrdinalIgnoreCase);
        Assert.Contains("clinic", message, StringComparison.OrdinalIgnoreCase);
    }

    [Fact]
    public void FromBodyKeepsOtherApiErrorsWithoutRawJsonWrapper()
    {
        var message = ApiErrorMessages.FromBody(
            """{"error":"Email already registered."}""",
            HttpStatusCode.BadRequest);

        Assert.Equal("Email already registered.", message);
    }

    [Fact]
    public void IsMissingClinicHeaderDetectsRequiredHeaderError()
    {
        Assert.True(ApiErrorMessages.IsMissingClinicHeader("X-Clinic-Id header is required."));
        Assert.False(ApiErrorMessages.IsMissingClinicHeader("Email already registered."));
    }
}
