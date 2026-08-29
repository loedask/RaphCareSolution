using RaphCare.Mobile.Core.Common.Services.Api;
using Xunit;

namespace RaphCare.Mobile.Tests;

public sealed class ClinicIdResolutionTests
{
    [Fact]
    public void ResolvePrefersFirstNonEmptyCandidate()
    {
        var selected = Guid.Parse("aaaaaaaa-aaaa-aaaa-aaaa-aaaaaaaaaaaa");
        var config = Guid.Parse("bbbbbbbb-bbbb-bbbb-bbbb-bbbbbbbbbbbb");

        var resolved = ClinicIdResolution.Resolve(selected, config, null);

        Assert.Equal(selected, resolved);
    }

    [Fact]
    public void ResolveSkipsEmptyGuidsAndFallsBack()
    {
        var config = Guid.Parse("11111111-1111-1111-1111-111111111101");

        var resolved = ClinicIdResolution.Resolve(null, Guid.Empty, config);

        Assert.Equal(config, resolved);
    }

    [Fact]
    public void ResolveReturnsNullWhenNoCandidates()
    {
        Assert.Null(ClinicIdResolution.Resolve(null, Guid.Empty));
    }
}
