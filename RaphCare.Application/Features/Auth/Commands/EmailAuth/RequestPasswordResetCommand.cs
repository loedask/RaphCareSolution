using MediatR;
using RaphCare.Application.Common.Interfaces;

namespace RaphCare.Application.Features.Auth.Commands.EmailAuth;

/// <summary>Sends a password-reset code when the email has a local password account. Always succeeds to the caller.</summary>
public sealed class RequestPasswordResetCommand : IRequest, IAllowAnonymousRequest
{
    public string Email { get; init; } = string.Empty;
}
