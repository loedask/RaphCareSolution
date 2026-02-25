namespace RaphCare.Domain.Identity;

public class LoginAudit
{
    public Guid Id { get; set; }
    public Guid UserId { get; set; }
    public DateTime LoginTime { get; set; }
    public string? IpAddress { get; set; }
    public string? UserAgent { get; set; }
    public bool Success { get; set; }
    public DateTime CreatedAt { get; set; }
    public DateTime? UpdatedAt { get; set; }

    public ApplicationUser User { get; set; } = null!;
}
