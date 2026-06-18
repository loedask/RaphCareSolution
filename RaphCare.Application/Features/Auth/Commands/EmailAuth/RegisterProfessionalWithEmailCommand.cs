using MediatR;
using RaphCare.Application.Common.Interfaces;

namespace RaphCare.Application.Features.Auth.Commands.EmailAuth;

public sealed class RegisterProfessionalWithEmailCommand : IRequest<EmailAuthResult>, IAllowAnonymousRequest
{
    public string FirstName { get; init; } = string.Empty;
    public string LastName { get; init; } = string.Empty;
    public string Email { get; init; } = string.Empty;
    public string Password { get; init; } = string.Empty;
}
