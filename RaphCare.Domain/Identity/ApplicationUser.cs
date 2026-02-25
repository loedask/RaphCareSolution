namespace RaphCare.Domain.Identity;

public class ApplicationUser
{
    public Guid Id { get; set; }
    public string EntraObjectId { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;
    public string DisplayName { get; set; } = string.Empty;
    public bool IsActive { get; set; }
    public bool IsDeleted { get; set; }
    public DateTime CreatedAt { get; set; }
    public DateTime? UpdatedAt { get; set; }

    public ICollection<UserRole> UserRoles { get; set; } = new List<UserRole>();
    public ICollection<UserSession> UserSessions { get; set; } = new List<UserSession>();
    public ICollection<RefreshTokenRecord> RefreshTokenRecords { get; set; } = new List<RefreshTokenRecord>();
    public ICollection<AuditLog> AuditLogs { get; set; } = new List<AuditLog>();
    public ICollection<LoginAudit> LoginAudits { get; set; } = new List<LoginAudit>();
}
