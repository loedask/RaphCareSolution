namespace RaphCare.Application.Features.PatientProfile.DTOs;

/// <summary>Demographics the signed-in patient may view and edit (<c>api/patient/profile</c>).</summary>
public sealed class MyPatientProfileDto
{
    public Guid PatientId { get; set; }
    public string FirstName { get; set; } = string.Empty;
    public string LastName { get; set; } = string.Empty;
    public string? Email { get; set; }
    public string? PhoneNumber { get; set; }
    public DateTime DateOfBirth { get; set; }
    public string Gender { get; set; } = string.Empty;
    public string? ProfilePhotoUrl { get; set; }
}
