using RaphCare.Application.Common.Interfaces;
using RaphCare.Application.Features.Auth.Commands.EmailAuth;
using RaphCare.Domain.Identity;
using Xunit;

namespace RaphCare.Application.Tests.Features.Auth.Commands.EmailAuth;

public sealed class ConfirmPasswordResetHandlerTests
{
    [Fact]
    public async Task InvalidCodeMustNotChangePassword()
    {
        var auth = new FakeEmailPasswordAuth();
        var otp = new FakeEmailOtp { ValidateResult = false };
        var handler = new ConfirmPasswordResetHandler(otp, auth);

        var result = await handler.Handle(
            new ConfirmPasswordResetCommand
            {
                Email = "demo.patient@raphcare.com",
                VerificationCode = "000000",
                NewPassword = "NewPass!234"
            },
            CancellationToken.None);

        Assert.False(result.Success);
        Assert.False(auth.ResetCalled);
        Assert.Contains("invalid", result.Error!, StringComparison.OrdinalIgnoreCase);
    }

    [Fact]
    public async Task ValidCodeResetsPasswordForEmailAccount()
    {
        var auth = new FakeEmailPasswordAuth();
        var otp = new FakeEmailOtp { ValidateResult = true };
        var handler = new ConfirmPasswordResetHandler(otp, auth);

        var result = await handler.Handle(
            new ConfirmPasswordResetCommand
            {
                Email = "demo.patient@raphcare.com",
                VerificationCode = "123456",
                NewPassword = "NewPass!234"
            },
            CancellationToken.None);

        Assert.True(result.Success);
        Assert.True(auth.ResetCalled);
        Assert.Equal("demo.patient@raphcare.com", auth.LastResetEmail);
        Assert.Equal("NewPass!234", auth.LastResetPassword);
    }

    private sealed class FakeEmailOtp : IEmailOtpService
    {
        public bool ValidateResult { get; set; }

        public Task<string> GenerateOtpAsync(string email, CancellationToken cancellationToken = default) =>
            Task.FromResult("123456");

        public Task<bool> ValidateOtpAsync(string email, string code, CancellationToken cancellationToken = default) =>
            Task.FromResult(ValidateResult);
    }

    private sealed class FakeEmailPasswordAuth : IEmailPasswordAuthService
    {
        public bool ResetCalled { get; private set; }
        public string? LastResetEmail { get; private set; }
        public string? LastResetPassword { get; private set; }

        public Task<(bool Success, string? Error, ApplicationUser? User, Guid PatientId)> RegisterAsync(
            string firstName, string lastName, string email, string password, Guid? clinicId,
            CancellationToken cancellationToken = default) =>
            throw new NotSupportedException();

        public Task<(bool Success, string? Error, ApplicationUser? User, Guid PatientId)> SignInAsync(
            string email, string password, CancellationToken cancellationToken = default) =>
            throw new NotSupportedException();

        public Task<(bool Success, string? Error, ApplicationUser? User)> RegisterProfessionalAsync(
            string firstName, string lastName, string email, string password,
            CancellationToken cancellationToken = default) =>
            throw new NotSupportedException();

        public Task<(bool Success, string? Error, ApplicationUser? User)> SignInProfessionalAsync(
            string email, string password, CancellationToken cancellationToken = default) =>
            throw new NotSupportedException();

        public Task<(bool Success, string? Error)> ChangePasswordAsync(
            Guid userId, string currentPassword, string newPassword,
            CancellationToken cancellationToken = default) =>
            throw new NotSupportedException();

        public Task<bool> HasEmailPasswordCredentialAsync(string email, CancellationToken cancellationToken = default) =>
            Task.FromResult(true);

        public Task<(bool Success, string? Error)> ResetPasswordByEmailAsync(
            string email, string newPassword, CancellationToken cancellationToken = default)
        {
            ResetCalled = true;
            LastResetEmail = email;
            LastResetPassword = newPassword;
            return Task.FromResult<(bool, string?)>((true, null));
        }
    }
}
