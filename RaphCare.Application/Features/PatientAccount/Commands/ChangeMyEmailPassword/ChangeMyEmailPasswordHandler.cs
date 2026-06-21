using FluentValidation.Results;
using MediatR;
using RaphCare.Application.Common.Exceptions;
using RaphCare.Application.Common.Interfaces;

namespace RaphCare.Application.Features.PatientAccount.Commands.ChangeMyEmailPassword;

public sealed class ChangeMyEmailPasswordHandler(
    IEmailPasswordAuthService emailPasswordAuth,
    ICurrentUserService currentUser) : IRequestHandler<ChangeMyEmailPasswordCommand, Unit>
{
    private readonly IEmailPasswordAuthService _emailPasswordAuth = emailPasswordAuth;
    private readonly ICurrentUserService _currentUser = currentUser;

    public async Task<Unit> Handle(ChangeMyEmailPasswordCommand request, CancellationToken cancellationToken)
    {
        var userId = _currentUser.CurrentUserId
            ?? throw new ForbiddenAccessException("Sign in required.");

        var (success, error) = await _emailPasswordAuth
            .ChangePasswordAsync(userId, request.CurrentPassword, request.NewPassword, cancellationToken)
            .ConfigureAwait(false);

        if (!success)
        {
            throw new ValidationException(new[]
            {
                new ValidationFailure(nameof(ChangeMyEmailPasswordCommand.CurrentPassword), error ?? "Password change failed.")
            });
        }

        return Unit.Value;
    }
}
