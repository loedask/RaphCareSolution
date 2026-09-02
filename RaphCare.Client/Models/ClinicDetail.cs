namespace RaphCare.Client.Models;

public sealed class ClinicDetail
{
    public Guid Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public string RegistrationNumber { get; set; } = string.Empty;
    public string ReferenceCode { get; set; } = string.Empty;
    public string Country { get; set; } = string.Empty;
    public string TimeZone { get; set; } = string.Empty;
    public bool IsActive { get; set; }
    public DateTime CreatedAt { get; set; }
    public Guid? RegisteredByApplicationUserId { get; set; }
    public bool CurrentUserIsAdministrator { get; set; }
    public bool CurrentUserCanDocumentVisits { get; set; }
    public bool CurrentUserCanRecordWardNotes { get; set; }
    public bool CurrentUserCanDispense { get; set; }
    public bool CurrentUserCanCompleteLabs { get; set; }
    public IReadOnlyList<FacilityListItem> Facilities { get; set; } = Array.Empty<FacilityListItem>();
}

public sealed class FacilityListItem
{
    public Guid Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public string Address { get; set; } = string.Empty;
    public string City { get; set; } = string.Empty;
    public string Country { get; set; } = string.Empty;
    public bool IsVirtual { get; set; }
}
