namespace RaphCare.Application.Features.Auth.Commands.EmailAuth;

public sealed class EmailAuthResult
{
    public bool Success { get; init; }
    public string? Token { get; init; }
    public string? Error { get; init; }

    /// <summary>When true, password was valid and a verification code was emailed; submit again with the code.</summary>
    public bool RequiresVerification { get; init; }
}
