namespace RaphCare.Application.Features.Auth.Queries.GetRegistrationClinics;

public sealed class RegistrationClinicDto
{
    public Guid Id { get; init; }
    public string Name { get; init; } = string.Empty;

    /// <summary>Platform join code (for example <c>RC-DEMCLN</c>). Prefer this over Guids in patient UX.</summary>
    public string ReferenceCode { get; init; } = string.Empty;
}
