namespace RaphCare.API.Controllers;

/// <summary>
/// Claim payload. <see cref="RegistrationNumber"/> accepts the platform hospital reference (for example RC-K7M3P2) or the hospital's own registration number.
/// </summary>
public sealed class ClaimClinicByRegistrationNumberRequest
{
    public string RegistrationNumber { get; set; } = string.Empty;
}
