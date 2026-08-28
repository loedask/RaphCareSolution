using MediatR;
using RaphCare.Application.Common.Exceptions;
using RaphCare.Application.Common.Interfaces;
using RaphCare.Application.Features.Organization.DTOs;
using RaphCare.Domain.Clinical;
using RaphCare.Domain.Organization;
using RaphCare.Domain.Patients;

namespace RaphCare.Application.Features.Organization.Commands.AdmitAdminClinicPatient;

public sealed class AdmitAdminClinicPatientHandler(
    ICurrentUserService currentUserService,
    IClinicStaffMembershipService clinicStaffMembershipService,
    IUserRoleAssignmentService roleAssignmentService,
    IPatientClinicAccessService patientClinicAccessService,
    IRepository<Patient> patientRepository,
    IRepository<Bed> bedRepository,
    IRepository<Room> roomRepository,
    IRepository<Ward> wardRepository,
    IRepository<Facility> facilityRepository,
    IRepository<InpatientAdmission> admissionRepository,
    IDateTimeProvider clock,
    IUnitOfWork unitOfWork)
    : IRequestHandler<AdmitAdminClinicPatientCommand, AdminClinicAdmissionDto?>
{
    public async Task<AdminClinicAdmissionDto?> Handle(
        AdmitAdminClinicPatientCommand request,
        CancellationToken cancellationToken)
    {
        if (!await AdminClinicAuthorization.IsClinicAdministratorAsync(
                currentUserService, clinicStaffMembershipService, roleAssignmentService, request.ClinicId, cancellationToken)
            .ConfigureAwait(false))
            throw new ForbiddenAccessException("Only hospital administrators can admit patients.");

        if (!await patientClinicAccessService
                .HasClinicAccessAsync(request.PatientId, request.ClinicId, cancellationToken)
                .ConfigureAwait(false))
            throw new BusinessRuleException("Patient does not have access to this hospital.");

        var patient = await patientRepository.GetByIdAsync(request.PatientId, cancellationToken).ConfigureAwait(false);
        if (patient is null || patient.IsDeleted)
            throw new BusinessRuleException("Patient not found.");

        var existingStay = await admissionRepository.SearchAsync(
            q => q.Where(a => a.ClinicId == request.ClinicId && a.PatientId == request.PatientId && a.Status == "Admitted"),
            1, 1, applyDefaultIdOrdering: false, cancellationToken).ConfigureAwait(false);
        if (existingStay.TotalCount > 0)
            throw new BusinessRuleException("Patient is already admitted at this hospital.");

        var bed = await bedRepository.GetByIdAsync(request.BedId, cancellationToken).ConfigureAwait(false);
        if (bed is null || !bed.IsActive)
            return null;

        var room = await roomRepository.GetByIdAsync(bed.RoomId, cancellationToken).ConfigureAwait(false);
        if (room is null)
            return null;

        var ward = await wardRepository.GetByIdAsync(room.WardId, cancellationToken).ConfigureAwait(false);
        if (ward is null || ward.ClinicId != request.ClinicId)
            return null;

        if (bed.Status != "Available")
            throw new BusinessRuleException("This bed is not available.");

        var bedOccupied = await admissionRepository.SearchAsync(
            q => q.Where(a => a.BedId == bed.Id && a.Status == "Admitted"),
            1, 1, applyDefaultIdOrdering: false, cancellationToken).ConfigureAwait(false);
        if (bedOccupied.TotalCount > 0)
            throw new BusinessRuleException("This bed already has an active admission.");

        var facility = await facilityRepository.GetByIdAsync(ward.FacilityId, cancellationToken).ConfigureAwait(false);

        var admission = new InpatientAdmission
        {
            ClinicId = request.ClinicId,
            PatientId = patient.Id,
            BedId = bed.Id,
            AdmittedAt = clock.UtcNow,
            Status = "Admitted",
            Reason = string.IsNullOrWhiteSpace(request.Reason) ? null : request.Reason.Trim(),
            Notes = string.IsNullOrWhiteSpace(request.Notes) ? null : request.Notes.Trim(),
            AdmittedByApplicationUserId = currentUserService.CurrentUserId
        };

        bed.Status = "Occupied";
        await admissionRepository.AddAsync(admission, cancellationToken).ConfigureAwait(false);
        await patientClinicAccessService
            .GrantEncounterAccessAsync(patient.Id, request.ClinicId, cancellationToken)
            .ConfigureAwait(false);
        await unitOfWork.SaveChangesAsync(cancellationToken).ConfigureAwait(false);

        return new AdminClinicAdmissionDto
        {
            Id = admission.Id,
            PatientId = patient.Id,
            PatientName = $"{patient.FirstName} {patient.LastName}".Trim(),
            BedId = bed.Id,
            BedLabel = bed.Label,
            RoomName = room.Name,
            WardName = ward.Name,
            FacilityName = facility?.Name ?? "Facility",
            AdmittedAt = admission.AdmittedAt,
            Status = admission.Status,
            Reason = admission.Reason,
            Notes = admission.Notes
        };
    }
}
