using RaphCare.Client.Contracts;
using RaphCare.Client.Models;

namespace RaphCare.Client.Contracts.Interfaces;

public interface IAdminClinicService
{
    Task<Response<IReadOnlyList<ClinicListItem>>> GetClinicsAsync(CancellationToken cancellationToken = default);
    Task<Response<ClinicDetail>> GetClinicAsync(Guid clinicId, CancellationToken cancellationToken = default);
    Task<Response<ClinicDetail>> UpdateClinicAsync(Guid clinicId, UpdateClinicRequest request, CancellationToken cancellationToken = default);
    Task<Response<PagedClinicPatients>> GetPatientsAsync(Guid clinicId, int pageNumber = 1, int pageSize = 20, string? search = null, CancellationToken cancellationToken = default);
    Task<Response<ClinicPatientDetail>> GetPatientDetailAsync(Guid clinicId, Guid patientId, CancellationToken cancellationToken = default);
    Task<Response<ClinicPatientListItem>> GrantPatientAccessAsync(Guid clinicId, string email, string? notes = null, CancellationToken cancellationToken = default);
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
    Task<Response<ClinicDashboard>> GetDashboardAsync(Guid clinicId, CancellationToken cancellationToken = default);
    Task<Response<IReadOnlyList<ClinicProviderListItem>>> GetProvidersAsync(Guid clinicId, CancellationToken cancellationToken = default);
    Task<Response<ClinicProviderDetail>> GetProviderDetailAsync(Guid clinicId, Guid providerId, CancellationToken cancellationToken = default);
    Task<Response<ClinicProviderListItem>> CreateProviderAsync(Guid clinicId, Guid userId, string? licenseNumber = null, CancellationToken cancellationToken = default);
    Task<Response<ClinicProviderSchedule>> CreateProviderScheduleAsync(Guid clinicId, Guid providerId, CreateProviderScheduleRequest request, CancellationToken cancellationToken = default);
    Task<Response<bool>> DeleteProviderScheduleAsync(Guid clinicId, Guid providerId, Guid scheduleId, CancellationToken cancellationToken = default);
    Task<Response<PagedClinicAppointments>> GetAppointmentsAsync(Guid clinicId, int pageNumber = 1, int pageSize = 20, DateTime? fromUtc = null, DateTime? toUtc = null, string? status = null, CancellationToken cancellationToken = default);
    Task<Response<ClinicAppointmentListItem>> BookAppointmentAsync(Guid clinicId, BookClinicAppointmentRequest request, CancellationToken cancellationToken = default);
    Task<Response<bool>> CancelAppointmentAsync(Guid clinicId, Guid appointmentId, CancellationToken cancellationToken = default);
    Task<Response<ClinicAppointmentListItem>> RescheduleAppointmentAsync(Guid clinicId, Guid appointmentId, RescheduleClinicAppointmentRequest request, CancellationToken cancellationToken = default);
    Task<Response<ClinicVisitDetail>> StartVisitAsync(Guid clinicId, Guid appointmentId, string? summary = null, CancellationToken cancellationToken = default);
    Task<Response<ClinicVisitDetail>> GetVisitAsync(Guid clinicId, Guid visitId, CancellationToken cancellationToken = default);
    Task<Response<ClinicVisitDetail>> CompleteVisitAsync(Guid clinicId, Guid visitId, string? summary = null, CancellationToken cancellationToken = default);
    Task<Response<ClinicVisitVital>> RecordVisitVitalAsync(Guid clinicId, Guid visitId, RecordVisitVitalRequest request, CancellationToken cancellationToken = default);
    Task<Response<IReadOnlyList<ClinicDeviceListItem>>> GetDevicesAsync(Guid clinicId, CancellationToken cancellationToken = default);
    Task<Response<bool>> RevokePatientAccessAsync(Guid clinicId, Guid patientId, CancellationToken cancellationToken = default);
}
