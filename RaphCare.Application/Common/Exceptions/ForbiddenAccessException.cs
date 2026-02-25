namespace RaphCare.Application.Common.Exceptions;

public class ForbiddenAccessException : Exception
{
    public ForbiddenAccessException(string? message = null)
        : base(message ?? "You do not have permission to perform this action.")
    {
    }
}

