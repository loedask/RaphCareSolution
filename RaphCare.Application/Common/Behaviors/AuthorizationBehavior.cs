using MediatR;
using RaphCare.Application.Common.Exceptions;
using RaphCare.Application.Common.Interfaces;

namespace RaphCare.Application.Common.Behaviors;

/// <summary>
/// MediatR pipeline behavior that enforces authenticated user before handler execution. Throws <see cref="ForbiddenAccessException"/> when not authenticated.
/// </summary>
public class AuthorizationBehavior<TRequest, TResponse> : IPipelineBehavior<TRequest, TResponse>
    where TRequest : notnull
{
    private readonly ICurrentUserService _currentUserService;

    /// <summary>Creates the authorization behavior.</summary>
    public AuthorizationBehavior(ICurrentUserService currentUserService)
    {
        _currentUserService = currentUserService;
    }

    /// <inheritdoc />
    public async Task<TResponse> Handle(
        TRequest request,
        RequestHandlerDelegate<TResponse> next,
        CancellationToken cancellationToken)
    {
        if (request is not IAllowAnonymousRequest
            && request is not IPlatformAdminRequest
            && !_currentUserService.IsAuthenticated)
        {
            throw new ForbiddenAccessException();
        }

        // Hook for per-request authorization rules (e.g., attributes, roles) in future.

        return await next();
    }
}

