namespace RaphCare.Application.Common.Interfaces;

/// <summary>
/// Provides access to the current clinic (tenant) context resolved from the HTTP pipeline.
/// </summary>
public interface IClinicContext
{
    Guid? ClinicId { get; }
}

