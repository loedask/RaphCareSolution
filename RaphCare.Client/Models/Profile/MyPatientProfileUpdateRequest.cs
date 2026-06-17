namespace RaphCare.Client.Models.Profile;

/// <summary>Body for <c>PUT api/patient/profile</c>.</summary>
public sealed class MyPatientProfileUpdateRequest
{
    public string FirstName { get; set; } = string.Empty;
    public string LastName { get; set; } = string.Empty;
    public string? Email { get; set; }
    public string? PhoneNumber { get; set; }
    public DateTime DateOfBirth { get; set; }
    public string Gender { get; set; } = string.Empty;
}
