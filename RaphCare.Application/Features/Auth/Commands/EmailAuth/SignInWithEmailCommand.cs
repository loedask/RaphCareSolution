using MediatR;

namespace RaphCare.Application.Features.Auth.Commands.EmailAuth;

public sealed class SignInWithEmailCommand : IRequest<EmailAuthResult>
{
    public string Email { get; init; } = string.Empty;
    public string Password { get; init; } = string.Empty;
}
