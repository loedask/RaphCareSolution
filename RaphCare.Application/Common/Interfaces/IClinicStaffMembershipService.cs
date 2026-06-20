namespace RaphCare.Application.Common.Interfaces;

public interface IClinicStaffMembershipService
{
    Task EnsureMembershipAsync(Guid applicationUserId, Guid clinicId, CancellationToken cancellationToken = default);
    Task<IReadOnlyList<Guid>> GetClinicIdsForUserAsync(Guid applicationUserId, CancellationToken cancellationToken = default);
    Task<bool> HasMembershipAsync(Guid applicationUserId, Guid clinicId, CancellationToken cancellationToken = default);
    Task<int> GetActiveStaffCountAsync(Guid clinicId, CancellationToken cancellationToken = default);
}
