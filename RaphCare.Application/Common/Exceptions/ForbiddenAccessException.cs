namespace RaphCare.Application.Common.Exceptions;

/// <summary>
/// Thrown when the current user is not authorized to perform the action. Mapped to HTTP 403 by API.
/// </summary>
public class ForbiddenAccessException : Exception
{
    public ForbiddenAccessException(string? message = null)
        : base(message ?? "You do not have permission to perform this action.")
    {
    }
}

