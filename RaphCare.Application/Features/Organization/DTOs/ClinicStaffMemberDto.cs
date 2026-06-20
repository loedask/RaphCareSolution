namespace RaphCare.Application.Features.Organization.DTOs;

public sealed class ClinicStaffMemberDto
{
    public Guid UserId { get; set; }
    public string Email { get; set; } = string.Empty;
    public string DisplayName { get; set; } = string.Empty;
    public IReadOnlyList<string> Roles { get; set; } = Array.Empty<string>();
    public DateTime JoinedAt { get; set; }
    public bool IsActive { get; set; }
}
