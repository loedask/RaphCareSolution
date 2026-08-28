using MediatR;
using RaphCare.Application.Common.Exceptions;
using RaphCare.Application.Common.Interfaces;
using RaphCare.Application.Features.Organization.DTOs;
using RaphCare.Domain.Clinical;
using RaphCare.Domain.Organization;
using RaphCare.Domain.Patients;

namespace RaphCare.Application.Features.Organization.Commands.DischargeAdminClinicAdmission;

public sealed class DischargeAdminClinicAdmissionHandler(
    ICurrentUserService currentUserService,
    IClinicStaffMembershipService clinicStaffMembershipService,
    IUserRoleAssignmentService roleAssignmentService,
    IRepository<InpatientAdmission> admissionRepository,
    IRepository<Bed> bedRepository,
    IRepository<Room> roomRepository,
    IRepository<Ward> wardRepository,
    IRepository<Facility> facilityRepository,
    IRepository<Patient> patientRepository,
    IDateTimeProvider clock,
    IUnitOfWork unitOfWork)
    : IRequestHandler<DischargeAdminClinicAdmissionCommand, AdminClinicAdmissionDto?>
{
    public async Task<AdminClinicAdmissionDto?> Handle(
        DischargeAdminClinicAdmissionCommand request,
        CancellationToken cancellationToken)
    {
        if (!await AdminClinicAuthorization.IsClinicAdministratorAsync(
                currentUserService, clinicStaffMembershipService, roleAssignmentService, request.ClinicId, cancellationToken)
            .ConfigureAwait(false))
            throw new ForbiddenAccessException("Only hospital administrators can discharge patients.");

        var admission = await admissionRepository.GetByIdAsync(request.AdmissionId, cancellationToken).ConfigureAwait(false);
        if (admission is null || admission.ClinicId != request.ClinicId)
            return null;

        if (admission.Status != "Admitted")
            throw new BusinessRuleException("Admission is already closed.");

        var bed = await bedRepository.GetByIdAsync(admission.BedId, cancellationToken).ConfigureAwait(false);
        if (bed is not null)
            bed.Status = "Available";

        admission.Status = "Discharged";
        admission.DischargedAt = clock.UtcNow;
        if (request.Notes is not null)
            admission.Notes = string.IsNullOrWhiteSpace(request.Notes) ? admission.Notes : request.Notes.Trim();

        await unitOfWork.SaveChangesAsync(cancellationToken).ConfigureAwait(false);

        var patient = await patientRepository.GetByIdAsync(admission.PatientId, cancellationToken).ConfigureAwait(false);
        var room = bed is null ? null : await roomRepository.GetByIdAsync(bed.RoomId, cancellationToken).ConfigureAwait(false);
        var ward = room is null ? null : await wardRepository.GetByIdAsync(room.WardId, cancellationToken).ConfigureAwait(false);
        var facility = ward is null ? null : await facilityRepository.GetByIdAsync(ward.FacilityId, cancellationToken).ConfigureAwait(false);

        return new AdminClinicAdmissionDto
        {
            Id = admission.Id,
            PatientId = admission.PatientId,
            PatientName = patient is null ? "Patient" : $"{patient.FirstName} {patient.LastName}".Trim(),
            BedId = admission.BedId,
            BedLabel = bed?.Label ?? "—",
            RoomName = room?.Name ?? "—",
            WardName = ward?.Name ?? "—",
            FacilityName = facility?.Name ?? "—",
            AdmittedAt = admission.AdmittedAt,
            DischargedAt = admission.DischargedAt,
            Status = admission.Status,
            Reason = admission.Reason,
            Notes = admission.Notes
        };
    }
}
