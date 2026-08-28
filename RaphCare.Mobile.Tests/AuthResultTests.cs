using RaphCare.Mobile.Core.Common.Models;
using Xunit;

namespace RaphCare.Mobile.Tests;

public class AuthResultTests
{
    [Fact]
    public void Ok_sets_success_and_tokens()
    {
        var expires = DateTimeOffset.UtcNow.AddHours(1);
        var r = AuthResult.Ok("at", "rt", expires);

        Assert.True(r.Success);
        Assert.Equal("at", r.AccessToken);
        Assert.Equal("rt", r.RefreshToken);
        Assert.Equal(expires, r.ExpiresOn);
        Assert.Null(r.ErrorMessage);
    }

    [Fact]
    public void Fail_sets_message()
    {
        var r = AuthResult.Fail("bad");

        Assert.False(r.Success);
        Assert.Equal("bad", r.ErrorMessage);
    }
}
