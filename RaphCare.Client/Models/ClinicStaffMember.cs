namespace RaphCare.Client.Models;

public sealed class ClinicStaffMember
{
    public Guid UserId { get; set; }
    public string Email { get; set; } = string.Empty;
    public string DisplayName { get; set; } = string.Empty;
    public IReadOnlyList<string> Roles { get; set; } = Array.Empty<string>();
    public DateTime JoinedAt { get; set; }
    public bool IsActive { get; set; }
    public bool HasLoggedIn { get; set; }
    public DateTime? LastInvitationSentAt { get; set; }
}

public sealed class SaveFacilityRequest
{
    public string Name { get; set; } = string.Empty;
    public string Address { get; set; } = string.Empty;
    public string City { get; set; } = string.Empty;
    public string Country { get; set; } = string.Empty;
    public bool IsVirtual { get; set; }
}
