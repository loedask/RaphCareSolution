namespace RaphCare.Application.Common.Interfaces;

public interface IOtpService
{
    Task<string> GenerateOtpAsync(string phoneNumber, CancellationToken cancellationToken = default);
    Task<bool> ValidateOtpAsync(string phoneNumber, string code, CancellationToken cancellationToken = default);
}

