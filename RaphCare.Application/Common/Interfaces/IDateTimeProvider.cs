namespace RaphCare.Application.Common.Interfaces;

/// <summary>
/// Abstraction over system clock for testability.
/// </summary>
public interface IDateTimeProvider
{
    DateTime UtcNow { get; }
}

