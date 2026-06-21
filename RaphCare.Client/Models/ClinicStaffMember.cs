namespace RaphCare.Client.Models;

public sealed class ClinicStaffMember
{
    public Guid? UserId { get; set; }
    public Guid? InvitationId { get; set; }
    public bool IsPendingInvitation { get; set; }
    public string Email { get; set; } = string.Empty;
    public string DisplayName { get; set; } = string.Empty;
    public IReadOnlyList<string> Roles { get; set; } = Array.Empty<string>();
    public DateTime JoinedAt { get; set; }
    public bool IsActive { get; set; }
    public bool HasLoggedIn { get; set; }
    public DateTime? LastInvitationSentAt { get; set; }
}

public sealed class UpdateClinicRequest
{
    public string Name { get; set; } = string.Empty;
    public string Country { get; set; } = string.Empty;
    public string TimeZone { get; set; } = string.Empty;
    public bool IsActive { get; set; } = true;
}

public sealed class ClinicPatientListItem
{
    public Guid PatientId { get; set; }
    public string FirstName { get; set; } = string.Empty;
    public string LastName { get; set; } = string.Empty;
    public DateTime DateOfBirth { get; set; }
    public string AccessType { get; set; } = string.Empty;
    public DateTime GrantedAt { get; set; }
}

public sealed class PagedClinicPatients
{
    public IReadOnlyList<ClinicPatientListItem> Items { get; set; } = Array.Empty<ClinicPatientListItem>();
    public int TotalCount { get; set; }
    public int PageNumber { get; set; }
    public int PageSize { get; set; }
}

public sealed class ClinicPatientDetail
{
    public Guid PatientId { get; set; }
    public string FirstName { get; set; } = string.Empty;
    public string LastName { get; set; } = string.Empty;
    public DateTime DateOfBirth { get; set; }
    public string Gender { get; set; } = string.Empty;
    public string? Email { get; set; }
    public string? PhoneNumber { get; set; }
    public string AccessType { get; set; } = string.Empty;
    public DateTime GrantedAt { get; set; }
    public string GrantedByRule { get; set; } = string.Empty;
    public string? Notes { get; set; }
    public IReadOnlyList<ClinicPatientVisitSummary> RecentVisits { get; set; } = Array.Empty<ClinicPatientVisitSummary>();
}

public sealed class ClinicPatientVisitSummary
{
    public Guid Id { get; set; }
    public DateTime VisitStart { get; set; }
    public DateTime? VisitEnd { get; set; }
    public string VisitType { get; set; } = string.Empty;
    public string Status { get; set; } = string.Empty;
    public string? Summary { get; set; }
}

public sealed class SaveFacilityRequest
{
    public string Name { get; set; } = string.Empty;
    public string Address { get; set; } = string.Empty;
    public string City { get; set; } = string.Empty;
    public string Country { get; set; } = string.Empty;
    public bool IsVirtual { get; set; }
}
