using MediatR;
using RaphCare.Application.Common.Interfaces;

namespace RaphCare.Application.Features.Auth.Commands.EmailAuth;

public sealed class ConfirmPasswordResetCommand : IRequest<EmailAuthResult>, IAllowAnonymousRequest
{
    public string Email { get; init; } = string.Empty;
    public string VerificationCode { get; init; } = string.Empty;
    public string NewPassword { get; init; } = string.Empty;
}
