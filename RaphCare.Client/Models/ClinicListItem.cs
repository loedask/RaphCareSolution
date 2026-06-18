namespace RaphCare.Client.Models;

public sealed class ClinicListItem
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
