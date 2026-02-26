namespace RaphCare.Domain.Identity;

/// <summary>
/// Join entity linking an application user to a role for role-based authorization.
/// </summary>
public class UserRole
{
    public Guid Id { get; set; }
    public Guid UserId { get; set; }
    public Guid RoleId { get; set; }
    public DateTime CreatedAt { get; set; }
    public DateTime? UpdatedAt { get; set; }

    public ApplicationUser User { get; set; } = null!;
    public Role Role { get; set; } = null!;
}
