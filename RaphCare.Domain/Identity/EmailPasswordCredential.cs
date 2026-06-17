namespace RaphCare.Domain.Identity;

/// <summary>
/// Local email/password credential linked to an application user.
/// </summary>
public class EmailPasswordCredential
{
    public Guid Id { get; set; }
    public Guid UserId { get; set; }
    public string Email { get; set; } = string.Empty;
    public string PasswordHash { get; set; } = string.Empty;
    public DateTime CreatedAt { get; set; }
    public DateTime? UpdatedAt { get; set; }

    public ApplicationUser User { get; set; } = null!;
}
