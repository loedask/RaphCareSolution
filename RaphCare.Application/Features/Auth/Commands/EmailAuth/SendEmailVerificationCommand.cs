using MediatR;
using RaphCare.Application.Common.Interfaces;

namespace RaphCare.Application.Features.Auth.Commands.EmailAuth;

public sealed class SendEmailVerificationCommand : IRequest, IAllowAnonymousRequest
{
    public string Email { get; init; } = string.Empty;
}
