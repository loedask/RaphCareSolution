using MediatR;

namespace RaphCare.Application.Features.Auth.Commands.EmailAuth;

public sealed class RegisterWithEmailCommand : IRequest<EmailAuthResult>
{
    public string FirstName { get; init; } = string.Empty;
    public string LastName { get; init; } = string.Empty;
    public string Email { get; init; } = string.Empty;
    public string Password { get; init; } = string.Empty;
}
