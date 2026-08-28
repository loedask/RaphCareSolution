namespace RaphCare.Application.Common.Interfaces;

public sealed class PendingStaffInvitationEntry
{
    public Guid InvitationId { get; init; }
    public string Email { get; init; } = string.Empty;
    public string JobRole { get; init; } = string.Empty;
    public DateTime InvitedAt { get; init; }
    public DateTime? LastInvitationSentAt { get; init; }
}

public interface IClinicStaffPendingInvitationService
{
    Task<PendingStaffInvitationEntry> CreateAsync(
        Guid clinicId,
        string email,
        Guid invitedByApplicationUserId,
        string jobRole,
        CancellationToken cancellationToken = default);

    Task<IReadOnlyList<PendingStaffInvitationEntry>> GetPendingForClinicAsync(
        Guid clinicId,
        CancellationToken cancellationToken = default);

    Task<bool> ResendAsync(Guid clinicId, Guid invitationId, CancellationToken cancellationToken = default);

    Task<bool> CancelAsync(Guid clinicId, Guid invitationId, CancellationToken cancellationToken = default);

    Task AcceptPendingInvitationsAsync(
        Guid applicationUserId,
        string email,
        CancellationToken cancellationToken = default);
}
