namespace RaphCare.Client.Models;

public sealed class RegisterClinicRequest
{
    public string Name { get; set; } = string.Empty;
    public string RegistrationNumber { get; set; } = string.Empty;
    public string Country { get; set; } = string.Empty;
    public string TimeZone { get; set; } = string.Empty;
    public string? FacilityName { get; set; }
    public string? FacilityAddress { get; set; }
    public string? FacilityCity { get; set; }
    public bool IsVirtualFacility { get; set; }
}
