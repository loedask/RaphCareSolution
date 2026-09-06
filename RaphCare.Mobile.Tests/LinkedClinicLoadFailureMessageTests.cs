using RaphCare.Mobile.Core.Common.Clinics;
using Xunit;

namespace RaphCare.Mobile.Tests;

/// <summary>
/// Regression: Active clinic used to always show a generic linked-hospitals error and hide API/network detail.
/// </summary>
public sealed class LinkedClinicLoadFailureMessageTests
{
    private const string Fallback = "Could not load linked hospitals. Try again later.";

    [Fact]
    public void PrefersApiErrorOverGenericFallback()
    {
        var message = LinkedClinicLoadFailureMessage.Resolve(
            "We couldn't reach the server. Check your connection and try again.",
            Fallback);

        Assert.Equal("We couldn't reach the server. Check your connection and try again.", message);
        Assert.DoesNotContain("Could not load linked hospitals", message, StringComparison.Ordinal);
    }

    [Theory]
    [InlineData(null)]
    [InlineData("")]
    [InlineData("   ")]
    public void FallsBackWhenApiErrorMissing(string? apiError) =>
        Assert.Equal(Fallback, LinkedClinicLoadFailureMessage.Resolve(apiError, Fallback));

    [Fact]
    public void TrimsApiError() =>
        Assert.Equal("Token expired.", LinkedClinicLoadFailureMessage.Resolve("  Token expired.  ", Fallback));
}
