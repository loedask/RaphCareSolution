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
        Assert.Contains("hospital", message, StringComparison.OrdinalIgnoreCase);
    }

    [Fact]
    public void FromBodyRewritesInvalidClinicHeaderToFriendlyMessage()
    {
        var message = ApiErrorMessages.FromBody(
            """{"error":"X-Clinic-Id must be a valid Guid."}""",
            HttpStatusCode.BadRequest);

        Assert.DoesNotContain("valid Guid", message, StringComparison.OrdinalIgnoreCase);
        Assert.Contains("hospital", message, StringComparison.OrdinalIgnoreCase);
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
    public void FromBodyPrefersValidationFieldErrorsOverGenericDetail()
    {
        var message = ApiErrorMessages.FromBody(
            """
            {
              "title": "Validation Error",
              "status": 400,
              "detail": "One or more validation failures occurred.",
              "errors": {
                "SerialNumber": [ "A device with this serial number is already in the fleet." ]
              }
            }
            """,
            HttpStatusCode.BadRequest);

        Assert.Equal("A device with this serial number is already in the fleet.", message);
        Assert.DoesNotContain("One or more validation failures", message, StringComparison.OrdinalIgnoreCase);
    }

    [Fact]
    public void IsClinicSelectionRequiredDetectsRewrittenMissingClinicMessage()
    {
        var rewritten = ApiErrorMessages.FromBody(
            """{"error":"X-Clinic-Id header is required."}""",
            HttpStatusCode.BadRequest);

        Assert.True(ApiErrorMessages.IsClinicSelectionRequired(rewritten));
        Assert.False(ApiErrorMessages.IsClinicSelectionRequired("Email already registered."));
    }

    [Fact]
    public void IsMissingClinicHeaderDetectsRequiredHeaderError()
    {
        Assert.True(ApiErrorMessages.IsMissingClinicHeader("X-Clinic-Id header is required."));
        Assert.False(ApiErrorMessages.IsMissingClinicHeader("Email already registered."));
    }
}
