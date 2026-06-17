namespace RaphCare.Application.Common.Exceptions;

/// <summary>
/// Thrown when a requested entity (e.g. patient, visit, invoice) does not exist. Mapped to HTTP 404 by API.
/// </summary>
public class NotFoundException : Exception
{
    public NotFoundException(string name, object key)
        : base($"{name} with key '{key}' was not found.")
    {
    }
}

