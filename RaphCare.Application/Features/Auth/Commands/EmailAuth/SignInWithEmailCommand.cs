using MediatR;
using RaphCare.Application.Common.Interfaces;

namespace RaphCare.Application.Features.Auth.Commands.EmailAuth;

public sealed class SignInWithEmailCommand : IRequest<EmailAuthResult>, IAllowAnonymousRequest
{
    public string Email { get; init; } = string.Empty;
    public string Password { get; init; } = string.Empty;

    /// <summary>When omitted, validates password and emails a verification code.</summary>
    public string? VerificationCode { get; init; }
}
