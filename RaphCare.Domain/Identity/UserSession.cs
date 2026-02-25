namespace RaphCare.Domain.Identity;

public class UserSession
{
    public Guid Id { get; set; }
    public Guid UserId { get; set; }
    public string JwtId { get; set; } = string.Empty;
    public DateTime ExpiresAt { get; set; }
    public bool Revoked { get; set; }
    public string? IpAddress { get; set; }
    public DateTime CreatedAt { get; set; }
    public DateTime? UpdatedAt { get; set; }

    public ApplicationUser User { get; set; } = null!;
}
