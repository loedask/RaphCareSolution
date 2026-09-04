using MediatR;
using RaphCare.Application.Common.Exceptions;
using RaphCare.Application.Common.Interfaces;
using RaphCare.Application.Features.MentalHealth.DTOs;
using RaphCare.Application.Features.Organization;
using RaphCare.Domain.MentalHealth;
using RaphCare.Domain.Patients;

namespace RaphCare.Application.Features.MentalHealth.Commands.CreateTherapySession;

public sealed class CreateTherapySessionHandler(
    ICurrentUserService currentUserService,
    IClinicStaffMembershipService clinicStaffMembershipService,
    IPatientClinicAccessService patientClinicAccessService,
    IRepository<Patient> patientRepository,
    IRepository<TherapySession> sessionRepository,
    IDateTimeProvider clock,
    IUnitOfWork unitOfWork)
    : IRequestHandler<CreateTherapySessionCommand, TherapySessionDto?>
{
    public async Task<TherapySessionDto?> Handle(
        CreateTherapySessionCommand request,
        CancellationToken cancellationToken)
    {
        await AdminClinicAuthorization.EnsureClinicStaffAsync(
                currentUserService,
                clinicStaffMembershipService,
                request.ClinicId,
                "Only hospital staff can log a therapy session.",
                cancellationToken)
            .ConfigureAwait(false);

        if (currentUserService.CurrentUserId is not Guid therapistUserId)
            throw new ForbiddenAccessException("Sign in is required.");

        if (!await patientClinicAccessService
                .HasClinicAccessAsync(request.PatientId, request.ClinicId, cancellationToken)
                .ConfigureAwait(false))
            throw new BusinessRuleException("Patient does not have access to this hospital.");

        var patient = await patientRepository.GetByIdAsync(request.PatientId, cancellationToken)
            .ConfigureAwait(false);
        if (patient is null || patient.IsDeleted)
            return null;

        var session = new TherapySession
        {
            ClinicId = request.ClinicId,
            PatientId = request.PatientId,
            TherapistId = therapistUserId,
            SessionStart = clock.UtcNow,
            SessionType = request.SessionType.Trim(),
            Status = "Completed",
            Summary = string.IsNullOrWhiteSpace(request.Summary) ? null : request.Summary.Trim(),
            IsConfidential = request.IsConfidential
        };

        await sessionRepository.AddAsync(session, cancellationToken).ConfigureAwait(false);
        await unitOfWork.SaveChangesAsync(cancellationToken).ConfigureAwait(false);

        return TherapyMapper.ToSessionDto(session);
    }
}
