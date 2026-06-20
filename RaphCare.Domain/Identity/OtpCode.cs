namespace RaphCare.Domain.Identity;

/// <summary>
/// One-time password (OTP) code for phone or email verification.
/// Stored as a hash, single-use, short-lived.
/// </summary>
public class OtpCode
{
    public Guid Id { get; set; }
    public string PhoneNumber { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;
    public string CodeHash { get; set; } = string.Empty;
    public DateTime ExpiresAt { get; set; }
    public bool IsUsed { get; set; }
    public DateTime CreatedAt { get; set; }
    public DateTime? UsedAt { get; set; }
}

