namespace RaphCare.Application.Common.Interfaces;

/// <summary>
/// Provides information about the current authenticated user.
/// </summary>
public interface ICurrentUserService
{
    string? UserId { get; }
    Guid? CurrentUserId { get; }
    string? UserName { get; }
    Guid? CurrentPatientId { get; }
    bool IsAuthenticated { get; }
    string? IpAddress { get; }
    string? UserAgent { get; }
}

