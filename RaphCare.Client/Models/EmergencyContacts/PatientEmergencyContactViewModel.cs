namespace RaphCare.Client.Models.EmergencyContacts;

public sealed class PatientEmergencyContactViewModel
{
    public Guid Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public string? Relationship { get; set; }
    public string? PhoneNumber { get; set; }
    public string? Email { get; set; }
}
