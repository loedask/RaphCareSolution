using RaphCare.Client.Contracts;

namespace RaphCare.Client.Contracts.Interfaces;

/// <summary>Phone OTP endpoints (<c>api/auth/otp</c>). Uses HTTP JSON directly so verify responses return the JWT (generated NSwag client omits the body).</summary>
public interface IOtpAuthService
{
    Task<Response<bool>> SendOtpAsync(string phoneNumber, CancellationToken cancellationToken = default);

    Task<Response<OtpVerifyResult>> VerifyOtpAsync(string phoneNumber, string code, CancellationToken cancellationToken = default);
}

/// <summary>Result of POST <c>api/auth/otp/verify</c>.</summary>
public sealed class OtpVerifyResult
{
    public bool Success { get; init; }
    public string? Token { get; init; }
}
