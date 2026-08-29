using RaphCare.Application.Common.Interfaces;
using RaphCare.Application.Features.Auth.Commands.EmailAuth;
using RaphCare.Domain.Identity;
using Xunit;

namespace RaphCare.Application.Tests.Features.Auth.Commands.EmailAuth;

public sealed class SignInWithEmailHandlerTests
{
    [Fact]
    public async Task DemoPatientSignInSkipsEmailOtpAndReturnsToken()
    {
        var auth = new FakeAuth();
        var otp = new FakeOtp();
        var mail = new FakeMail();
        var tokens = new FakeTokens();
        var handler = new SignInWithEmailHandler(auth, otp, mail, tokens);

        var result = await handler.Handle(
            new SignInWithEmailCommand
            {
                Email = DemoPackAccounts.PatientEmail,
                Password = "RaphCareDemo!2026"
            },
            CancellationToken.None);

        Assert.True(result.Success);
        Assert.False(result.RequiresVerification);
        Assert.Equal("patient-token", result.Token);
        Assert.False(otp.Generated);
        Assert.False(mail.Sent);
    }

    [Fact]
    public async Task LegacyDemoDomainSignInAlsoSkipsEmailOtp()
    {
        var auth = new FakeAuth();
        var otp = new FakeOtp();
        var mail = new FakeMail();
        var tokens = new FakeTokens();
        var handler = new SignInWithEmailHandler(auth, otp, mail, tokens);

        var result = await handler.Handle(
            new SignInWithEmailCommand
            {
                Email = "demo.patient@raphcare.demo",
                Password = "RaphCareDemo!2026"
            },
            CancellationToken.None);

        Assert.True(result.Success);
        Assert.False(result.RequiresVerification);
        Assert.Equal("patient-token", result.Token);
        Assert.False(mail.Sent);
    }

    [Fact]
    public async Task RealPatientSignInRequiresEmailOtp()
    {
        var auth = new FakeAuth();
        var otp = new FakeOtp();
        var mail = new FakeMail();
        var tokens = new FakeTokens();
        var handler = new SignInWithEmailHandler(auth, otp, mail, tokens);

        var result = await handler.Handle(
            new SignInWithEmailCommand
            {
                Email = "patient@example.com",
                Password = "Password1!"
            },
            CancellationToken.None);

        Assert.True(result.Success);
        Assert.True(result.RequiresVerification);
        Assert.Null(result.Token);
        Assert.True(otp.Generated);
        Assert.True(mail.Sent);
    }

    private sealed class FakeAuth : IEmailPasswordAuthService
    {
        public Task<(bool Success, string? Error, ApplicationUser? User, Guid PatientId)> RegisterAsync(
            string firstName, string lastName, string email, string password, Guid? clinicId,
            CancellationToken cancellationToken = default) => throw new NotSupportedException();

        public Task<(bool Success, string? Error, ApplicationUser? User, Guid PatientId)> SignInAsync(
            string email, string password, CancellationToken cancellationToken = default) =>
            Task.FromResult<(bool, string?, ApplicationUser?, Guid)>((
                true,
                null,
                new ApplicationUser { Id = Guid.NewGuid(), Email = email.Trim().ToLowerInvariant() },
                Guid.Parse("11111111-1111-1111-1111-111111111104")));

        public Task<(bool Success, string? Error, ApplicationUser? User)> RegisterProfessionalAsync(
            string firstName, string lastName, string email, string password,
            CancellationToken cancellationToken = default) => throw new NotSupportedException();

        public Task<(bool Success, string? Error, ApplicationUser? User)> SignInProfessionalAsync(
            string email, string password, CancellationToken cancellationToken = default) =>
            throw new NotSupportedException();

        public Task<(bool Success, string? Error)> ChangePasswordAsync(
            Guid userId, string currentPassword, string newPassword,
            CancellationToken cancellationToken = default) => throw new NotSupportedException();

        public Task<bool> HasEmailPasswordCredentialAsync(string email, CancellationToken cancellationToken = default) =>
            Task.FromResult(true);

        public Task<(bool Success, string? Error)> ResetPasswordByEmailAsync(
            string email, string newPassword, CancellationToken cancellationToken = default) =>
            throw new NotSupportedException();
    }

    private sealed class FakeOtp : IEmailOtpService
    {
        public bool Generated { get; private set; }

        public Task<string> GenerateOtpAsync(string email, CancellationToken cancellationToken = default)
        {
            Generated = true;
            return Task.FromResult("123456");
        }

        public Task<bool> ValidateOtpAsync(string email, string code, CancellationToken cancellationToken = default) =>
            Task.FromResult(true);
    }

    private sealed class FakeMail : IEmailService
    {
        public bool Sent { get; private set; }

        public Task SendEmailAsync(string recipient, string subject, string body, CancellationToken cancellationToken = default)
        {
            Sent = true;
            return Task.CompletedTask;
        }

        public Task SendEmailAsync(
            string recipient,
            string subject,
            string plainBody,
            string htmlBody,
            CancellationToken cancellationToken = default)
        {
            Sent = true;
            return Task.CompletedTask;
        }
    }

    private sealed class FakeTokens : ITokenService
    {
        public string GeneratePatientToken(ApplicationUser user, Guid patientId) => "patient-token";

        public string GenerateStaffToken(ApplicationUser user, IReadOnlyList<string> roles) => "staff-token";
    }
}
