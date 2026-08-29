namespace RaphCare.Application.Features.Appointments.DTOs;

/// <summary>Provider a patient can pick when booking (name only; no staff-only fields).</summary>
public sealed class BookableProviderDto
{
    public Guid Id { get; init; }
    public string DisplayName { get; init; } = string.Empty;
}
