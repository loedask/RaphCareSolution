using MediatR;
using RaphCare.Application.Common.Exceptions;
using RaphCare.Application.Common.Interfaces;
using RaphCare.Application.Features.Organization.DTOs;
using RaphCare.Domain.Clinical;

namespace RaphCare.Application.Features.Organization.Commands.CreateAdminClinicAdmissionObservation;

public sealed class CreateAdminClinicAdmissionObservationHandler(
    ICurrentUserService currentUserService,
    IClinicStaffMembershipService clinicStaffMembershipService,
    IUserRoleAssignmentService roleAssignmentService,
    IRepository<InpatientAdmission> admissionRepository,
    IRepository<InpatientObservation> observationRepository,
    IDateTimeProvider clock,
    IUnitOfWork unitOfWork)
    : IRequestHandler<CreateAdminClinicAdmissionObservationCommand, AdminClinicAdmissionObservationDto?>
{
    public async Task<AdminClinicAdmissionObservationDto?> Handle(
        CreateAdminClinicAdmissionObservationCommand request,
        CancellationToken cancellationToken)
    {
        if (!await AdminClinicAuthorization.CanRecordWardNotesAsync(
                currentUserService,
                clinicStaffMembershipService,
                roleAssignmentService,
                request.ClinicId,
                cancellationToken)
            .ConfigureAwait(false))
            throw new ForbiddenAccessException("Only a nurse, doctor, or hospital administrator can add a ward note.");

        var admission = await admissionRepository.GetByIdAsync(request.AdmissionId, cancellationToken).ConfigureAwait(false);
        if (admission is null || admission.ClinicId != request.ClinicId)
            return null;

        if (admission.Status != "Admitted")
            throw new BusinessRuleException("Cannot add a ward note after discharge.");

        var observation = new InpatientObservation
        {
            ClinicId = request.ClinicId,
            AdmissionId = admission.Id,
            RecordedAt = clock.UtcNow,
            RecordedByApplicationUserId = currentUserService.CurrentUserId,
            Note = request.Note.Trim(),
            HeartRate = request.HeartRate,
            TemperatureCelsius = request.TemperatureCelsius,
            OxygenSaturation = request.OxygenSaturation,
            SystolicBp = request.SystolicBp,
            DiastolicBp = request.DiastolicBp
        };

        await observationRepository.AddAsync(observation, cancellationToken).ConfigureAwait(false);
        await unitOfWork.SaveChangesAsync(cancellationToken).ConfigureAwait(false);

        return new AdminClinicAdmissionObservationDto
        {
            Id = observation.Id,
            AdmissionId = observation.AdmissionId,
            RecordedAt = observation.RecordedAt,
            Note = observation.Note,
            HeartRate = observation.HeartRate,
            TemperatureCelsius = observation.TemperatureCelsius,
            OxygenSaturation = observation.OxygenSaturation,
            SystolicBp = observation.SystolicBp,
            DiastolicBp = observation.DiastolicBp
        };
    }
}
