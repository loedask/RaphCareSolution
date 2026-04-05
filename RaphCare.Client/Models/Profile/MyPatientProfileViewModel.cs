namespace RaphCare.Client.Models.Profile;

/// <summary>Demographics returned by <c>GET api/patient/profile</c>.</summary>
public sealed class MyPatientProfileViewModel
{
    public Guid PatientId { get; set; }
    public string FirstName { get; set; } = string.Empty;
    public string LastName { get; set; } = string.Empty;
    public string? Email { get; set; }
    public string? PhoneNumber { get; set; }
    public DateTime DateOfBirth { get; set; }
    public string Gender { get; set; } = string.Empty;
}
