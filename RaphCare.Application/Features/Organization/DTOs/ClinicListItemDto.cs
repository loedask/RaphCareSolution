namespace RaphCare.Application.Features.Organization.DTOs;

/// <summary>Admin list row for a registered clinic (hospital).</summary>
public class ClinicListItemDto
{
    public Guid Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public string RegistrationNumber { get; set; } = string.Empty;
    public string Country { get; set; } = string.Empty;
    public string TimeZone { get; set; } = string.Empty;
    public bool IsActive { get; set; }
    public int FacilityCount { get; set; }
    public DateTime CreatedAt { get; set; }
}
