using MediatR;
using RaphCare.Application.Common.Exceptions;
using RaphCare.Application.Common.Interfaces;
using RaphCare.Application.Features.Organization.DTOs;
using RaphCare.Domain.Clinical;
using RaphCare.Domain.Organization;
using RaphCare.Domain.Patients;

namespace RaphCare.Application.Features.Organization.Commands.TransferAdminClinicAdmission;

public sealed class TransferAdminClinicAdmissionHandler(
    ICurrentUserService currentUserService,
    IClinicStaffMembershipService clinicStaffMembershipService,
    IUserRoleAssignmentService roleAssignmentService,
    IRepository<InpatientAdmission> admissionRepository,
    IRepository<Bed> bedRepository,
    IRepository<Room> roomRepository,
    IRepository<Ward> wardRepository,
    IRepository<Facility> facilityRepository,
    IRepository<Patient> patientRepository,
    IUnitOfWork unitOfWork)
    : IRequestHandler<TransferAdminClinicAdmissionCommand, AdminClinicAdmissionDto?>
{
    public async Task<AdminClinicAdmissionDto?> Handle(
        TransferAdminClinicAdmissionCommand request,
        CancellationToken cancellationToken)
    {
        if (!await AdminClinicAuthorization.IsClinicAdministratorAsync(
                currentUserService, clinicStaffMembershipService, roleAssignmentService, request.ClinicId, cancellationToken)
            .ConfigureAwait(false))
            throw new ForbiddenAccessException("Only hospital administrators can transfer patients.");

        var admission = await admissionRepository.GetByIdAsync(request.AdmissionId, cancellationToken).ConfigureAwait(false);
        if (admission is null || admission.ClinicId != request.ClinicId)
            return null;

        if (admission.Status != "Admitted")
            throw new BusinessRuleException("Only active admissions can be transferred.");

        if (admission.BedId == request.TargetBedId)
            throw new BusinessRuleException("Patient is already in that bed.");

        var fromBed = await bedRepository.GetByIdAsync(admission.BedId, cancellationToken).ConfigureAwait(false);
        if (fromBed is null)
            throw new BusinessRuleException("Current bed was not found.");

        var toBed = await bedRepository.GetByIdAsync(request.TargetBedId, cancellationToken).ConfigureAwait(false);
        if (toBed is null || !toBed.IsActive)
            return null;

        var toRoom = await roomRepository.GetByIdAsync(toBed.RoomId, cancellationToken).ConfigureAwait(false);
        if (toRoom is null || !toRoom.IsActive)
            return null;

        var toWard = await wardRepository.GetByIdAsync(toRoom.WardId, cancellationToken).ConfigureAwait(false);
        if (toWard is null || toWard.ClinicId != request.ClinicId || !toWard.IsActive)
            return null;

        if (toBed.Status != "Available")
            throw new BusinessRuleException("Target bed is not available.");

        var targetOccupied = await admissionRepository.SearchAsync(
            q => q.Where(a => a.BedId == toBed.Id && a.Status == "Admitted"),
            1, 1, applyDefaultIdOrdering: false, cancellationToken).ConfigureAwait(false);
        if (targetOccupied.TotalCount > 0)
            throw new BusinessRuleException("Target bed already has an active admission.");

        fromBed.Status = "Available";
        toBed.Status = "Occupied";
        admission.BedId = toBed.Id;

        if (!string.IsNullOrWhiteSpace(request.Notes))
        {
            var note = request.Notes.Trim();
            var combined = string.IsNullOrWhiteSpace(admission.Notes) ? note : $"{admission.Notes}\n{note}";
            admission.Notes = combined.Length <= 1000 ? combined : combined[..1000];
        }

        await unitOfWork.SaveChangesAsync(cancellationToken).ConfigureAwait(false);

        var patient = await patientRepository.GetByIdAsync(admission.PatientId, cancellationToken).ConfigureAwait(false);
        var facility = await facilityRepository.GetByIdAsync(toWard.FacilityId, cancellationToken).ConfigureAwait(false);

        return new AdminClinicAdmissionDto
        {
            Id = admission.Id,
            PatientId = admission.PatientId,
            PatientName = patient is null ? "Patient" : $"{patient.FirstName} {patient.LastName}".Trim(),
            BedId = toBed.Id,
            BedLabel = toBed.Label,
            RoomName = toRoom.Name,
            WardName = toWard.Name,
            FacilityName = facility?.Name ?? "Facility",
            AdmittedAt = admission.AdmittedAt,
            DischargedAt = admission.DischargedAt,
            Status = admission.Status,
            Reason = admission.Reason,
            Notes = admission.Notes
        };
    }
}
