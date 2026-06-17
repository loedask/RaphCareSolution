namespace RaphCare.Client.Models.Family;

public sealed class PatientFamilyMemberViewModel
{
    public Guid Id { get; set; }
    public string FirstName { get; set; } = string.Empty;
    public string LastName { get; set; } = string.Empty;
    public string Relationship { get; set; } = string.Empty;
    public DateTime? DateOfBirth { get; set; }
    public string? PhoneNumber { get; set; }
    public string? Email { get; set; }
    public Guid? LinkedPatientId { get; set; }

    public string DisplayName => $"{FirstName} {LastName}".Trim();
}
