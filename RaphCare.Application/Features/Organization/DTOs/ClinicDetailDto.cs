namespace RaphCare.Application.Features.Organization.DTOs;

public sealed class ClinicDetailDto
{
    public Guid Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public string RegistrationNumber { get; set; } = string.Empty;
    public string ReferenceCode { get; set; } = string.Empty;
    public string Country { get; set; } = string.Empty;
    public string TimeZone { get; set; } = string.Empty;
    public bool IsActive { get; set; }
    public bool AllowAiDischargeDraft { get; set; }
    public bool AllowAiMentalHealthNotes { get; set; }
    public DateTime CreatedAt { get; set; }
    public Guid? RegisteredByApplicationUserId { get; set; }
    public bool CurrentUserIsAdministrator { get; set; }
    public bool CurrentUserCanDocumentVisits { get; set; }
    public bool CurrentUserCanRecordWardNotes { get; set; }
    public bool CurrentUserCanDispense { get; set; }
    public bool CurrentUserCanCompleteLabs { get; set; }
    public IReadOnlyList<FacilityListItemDto> Facilities { get; set; } = Array.Empty<FacilityListItemDto>();
}

public sealed class FacilityListItemDto
{
    public Guid Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public string Address { get; set; } = string.Empty;
    public string City { get; set; } = string.Empty;
    public string Country { get; set; } = string.Empty;
    public bool IsVirtual { get; set; }
}
