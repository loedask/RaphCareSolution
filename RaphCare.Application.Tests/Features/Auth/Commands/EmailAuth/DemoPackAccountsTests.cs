using RaphCare.Domain.Identity;
using Xunit;

namespace RaphCare.Application.Tests.Features.Auth.Commands.EmailAuth;

public sealed class DemoPackAccountsTests
{
    [Theory]
    [InlineData("demo.patient@raphcare.com")]
    [InlineData("demo.admin@raphcare.com")]
    [InlineData("  Demo.Patient@RaphCare.com  ")]
    [InlineData("demo.patient@raphcare.demo")]
    [InlineData("demo.admin@raphcare.demo")]
    public void IsDemoEmailRecognizesCanonicalAndLegacyAddresses(string email) =>
        Assert.True(DemoPackAccounts.IsDemoEmail(email));

    [Theory]
    [InlineData("patient@example.com")]
    [InlineData("demo.patient@elsewhere.com")]
    [InlineData("")]
    [InlineData(null)]
    public void IsDemoEmailRejectsNonDemoAddresses(string? email) =>
        Assert.False(DemoPackAccounts.IsDemoEmail(email));
}
