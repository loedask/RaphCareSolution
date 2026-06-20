namespace RaphCare.Application.Common.Interfaces;

public sealed class ClinicStaffMembershipEntry
{
    public Guid ApplicationUserId { get; init; }
    public DateTime JoinedAt { get; init; }
    public bool IsActive { get; init; }
}

public interface IClinicStaffMembershipService
{
    Task EnsureMembershipAsync(Guid applicationUserId, Guid clinicId, CancellationToken cancellationToken = default);
    Task<IReadOnlyList<Guid>> GetClinicIdsForUserAsync(Guid applicationUserId, CancellationToken cancellationToken = default);
    Task<bool> HasMembershipAsync(Guid applicationUserId, Guid clinicId, CancellationToken cancellationToken = default);
    Task<int> GetActiveStaffCountAsync(Guid clinicId, CancellationToken cancellationToken = default);
    Task<IReadOnlyList<ClinicStaffMembershipEntry>> GetStaffMembershipsAsync(
        Guid clinicId,
        CancellationToken cancellationToken = default);
    Task<bool> DeactivateMembershipAsync(
        Guid applicationUserId,
        Guid clinicId,
        CancellationToken cancellationToken = default);
}
