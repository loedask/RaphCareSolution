using RaphCare.Client.Contracts;
using RaphCare.Client.Models;

namespace RaphCare.Client.Contracts.Interfaces;

public interface IAdminClinicService
{
    Task<Response<IReadOnlyList<ClinicListItem>>> GetClinicsAsync(CancellationToken cancellationToken = default);
    Task<Response<ClinicDetail>> GetClinicAsync(Guid clinicId, CancellationToken cancellationToken = default);
    Task<Response<ClinicDetail>> UpdateClinicAsync(Guid clinicId, UpdateClinicRequest request, CancellationToken cancellationToken = default);
    Task<Response<PagedClinicPatients>> GetPatientsAsync(Guid clinicId, int pageNumber = 1, int pageSize = 20, string? search = null, CancellationToken cancellationToken = default);
    Task<Response<bool>> EnsureMembershipAsync(Guid clinicId, CancellationToken cancellationToken = default);
    Task<Response<Guid?>> ClaimByRegistrationNumberAsync(string registrationNumber, CancellationToken cancellationToken = default);
    Task<Response<RegisterClinicResult>> RegisterClinicAsync(RegisterClinicRequest request, CancellationToken cancellationToken = default);
    Task<Response<IReadOnlyList<ClinicStaffMember>>> GetStaffAsync(Guid clinicId, CancellationToken cancellationToken = default);
    Task<Response<ClinicStaffMember>> InviteStaffAsync(Guid clinicId, string email, CancellationToken cancellationToken = default);
    Task<Response<bool>> ResendStaffInvitationAsync(Guid clinicId, Guid userId, CancellationToken cancellationToken = default);
    Task<Response<bool>> ResendPendingInvitationAsync(Guid clinicId, Guid invitationId, CancellationToken cancellationToken = default);
    Task<Response<bool>> CancelPendingInvitationAsync(Guid clinicId, Guid invitationId, CancellationToken cancellationToken = default);
    Task<Response<bool>> RemoveStaffAsync(Guid clinicId, Guid userId, CancellationToken cancellationToken = default);
    Task<Response<bool>> UpdateStaffRoleAsync(Guid clinicId, Guid userId, bool isAdministrator, CancellationToken cancellationToken = default);
    Task<Response<FacilityListItem>> CreateFacilityAsync(Guid clinicId, SaveFacilityRequest request, CancellationToken cancellationToken = default);
    Task<Response<FacilityListItem>> UpdateFacilityAsync(Guid clinicId, Guid facilityId, SaveFacilityRequest request, CancellationToken cancellationToken = default);
    Task<Response<bool>> DeleteFacilityAsync(Guid clinicId, Guid facilityId, CancellationToken cancellationToken = default);
}
