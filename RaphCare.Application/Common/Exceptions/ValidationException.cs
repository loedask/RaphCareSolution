using FluentValidation.Results;

namespace RaphCare.Application.Common.Exceptions;

/// <summary>
/// Thrown when request validation fails (e.g. FluentValidation). Errors dictionary contains property names and messages. Mapped to HTTP 400.
/// </summary>
public class ValidationException : Exception
{
    /// <summary>Validation errors keyed by property name.</summary>
    public IDictionary<string, string[]> Errors { get; }

    public ValidationException(IEnumerable<ValidationFailure> failures)
        : base("One or more validation failures have occurred.")
    {
        Errors = failures
            .GroupBy(f => f.PropertyName)
            .ToDictionary(
                g => g.Key,
                g => g.Select(f => f.ErrorMessage).ToArray());
    }
}

