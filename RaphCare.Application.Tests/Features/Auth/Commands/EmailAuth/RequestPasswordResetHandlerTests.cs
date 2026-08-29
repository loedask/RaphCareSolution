using RaphCare.Application.Common.Interfaces;
using RaphCare.Application.Features.Auth.Commands.EmailAuth;
using RaphCare.Domain.Identity;
using Xunit;

namespace RaphCare.Application.Tests.Features.Auth.Commands.EmailAuth;

public sealed class RequestPasswordResetHandlerTests
{
    [Fact]
    public async Task UnknownEmailMustNotSendMailOrRevealAccount()
    {
        var auth = new FakeAuth { HasCredential = false };
        var otp = new FakeOtp();
        var mail = new FakeMail();
        var handler = new RequestPasswordResetHandler(auth, otp, mail);

        await handler.Handle(
            new RequestPasswordResetCommand { Email = "nobody@example.com" },
            CancellationToken.None);

        Assert.False(otp.Generated);
        Assert.False(mail.Sent);
    }

    [Fact]
    public async Task KnownEmailSendsPasswordResetCode()
    {
        var auth = new FakeAuth { HasCredential = true };
        var otp = new FakeOtp();
        var mail = new FakeMail();
        var handler = new RequestPasswordResetHandler(auth, otp, mail);

        await handler.Handle(
            new RequestPasswordResetCommand { Email = "patient@example.com" },
            CancellationToken.None);

        Assert.True(otp.Generated);
        Assert.True(mail.Sent);
        Assert.Equal("Reset your RaphCare password", mail.LastSubject);
    }

    [Fact]
    public async Task DemoEmailMustNotSendResetMailBecauseMailboxDoesNotExist()
    {
        var auth = new FakeAuth { HasCredential = true };
        var otp = new FakeOtp();
        var mail = new FakeMail();
        var handler = new RequestPasswordResetHandler(auth, otp, mail);

        await handler.Handle(
            new RequestPasswordResetCommand { Email = DemoPackAccounts.PatientEmail },
            CancellationToken.None);

        Assert.False(otp.Generated);
        Assert.False(mail.Sent);
    }

    private sealed class FakeAuth : IEmailPasswordAuthService
    {
        public bool HasCredential { get; set; }

        public Task<(bool Success, string? Error, ApplicationUser? User, Guid PatientId)> RegisterAsync(
            string firstName, string lastName, string email, string password, Guid? clinicId,
            CancellationToken cancellationToken = default) => throw new NotSupportedException();

        public Task<(bool Success, string? Error, ApplicationUser? User, Guid PatientId)> SignInAsync(
            string email, string password, CancellationToken cancellationToken = default) =>
            throw new NotSupportedException();

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
            Task.FromResult(HasCredential);

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
            return Task.FromResult("654321");
        }

        public Task<bool> ValidateOtpAsync(string email, string code, CancellationToken cancellationToken = default) =>
            Task.FromResult(true);
    }

    private sealed class FakeMail : IEmailService
    {
        public bool Sent { get; private set; }
        public string? LastSubject { get; private set; }

        public Task SendEmailAsync(string recipient, string subject, string body, CancellationToken cancellationToken = default)
        {
            Sent = true;
            LastSubject = subject;
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
            LastSubject = subject;
            return Task.CompletedTask;
        }
    }
}
