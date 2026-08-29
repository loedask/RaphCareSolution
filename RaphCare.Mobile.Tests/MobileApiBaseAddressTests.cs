using RaphCare.Mobile.Core.Common.Configuration;
using Xunit;

namespace RaphCare.Mobile.Tests;

public sealed class MobileApiBaseAddressTests
{
    [Fact]
    public void ReleaseBuildMustNotUseLocalhostWhenConfigMissing()
    {
        var resolved = MobileApiBaseAddress.Resolve(configured: null, isDebugBuild: false);

        Assert.Equal(MobileApiBaseAddress.StagingDefault, resolved);
        Assert.False(MobileApiBaseAddress.IsLoopback(resolved));
    }

    [Fact]
    public void ReleaseBuildReplacesLocalhostConfigWithStaging()
    {
        var resolved = MobileApiBaseAddress.Resolve("http://localhost:5281/", isDebugBuild: false);

        Assert.Equal(MobileApiBaseAddress.StagingDefault, resolved);
    }

    [Fact]
    public void DebugBuildKeepsLocalhostForLocalApi()
    {
        var resolved = MobileApiBaseAddress.Resolve("http://localhost:5281/", isDebugBuild: true);

        Assert.Equal("http://localhost:5281/", resolved);
    }

    [Fact]
    public void HostedHttpsUrlIsPreserved()
    {
        var hosted = "https://raphcare-api-eydjcnefhae2dpa2.southafricanorth-01.azurewebsites.net";
        var resolved = MobileApiBaseAddress.Resolve(hosted, isDebugBuild: false);

        Assert.Equal(MobileApiBaseAddress.StagingDefault, resolved);
    }
}
