namespace RaphCare.Application.Features.Auth.Commands.EmailAuth;

public sealed class EmailAuthResult
{
    public bool Success { get; init; }
    public string? Token { get; init; }
    public string? Error { get; init; }
}
