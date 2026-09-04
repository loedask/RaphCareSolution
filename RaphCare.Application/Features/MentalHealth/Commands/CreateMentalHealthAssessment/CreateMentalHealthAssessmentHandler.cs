using MediatR;
using RaphCare.Application.Common.Exceptions;
using RaphCare.Application.Common.Interfaces;
using RaphCare.Application.Features.MentalHealth.DTOs;
using RaphCare.Application.Features.Organization;
using RaphCare.Domain.MentalHealth;
using RaphCare.Domain.Patients;

namespace RaphCare.Application.Features.MentalHealth.Commands.CreateMentalHealthAssessment;

public sealed class CreateMentalHealthAssessmentHandler(
    ICurrentUserService currentUserService,
    IClinicStaffMembershipService clinicStaffMembershipService,
    IPatientClinicAccessService patientClinicAccessService,
    IRepository<Patient> patientRepository,
    IRepository<MentalHealthAssessment> assessmentRepository,
    IDateTimeProvider clock,
    IUnitOfWork unitOfWork)
    : IRequestHandler<CreateMentalHealthAssessmentCommand, MentalHealthAssessmentDetailDto?>
{
    public async Task<MentalHealthAssessmentDetailDto?> Handle(
        CreateMentalHealthAssessmentCommand request,
        CancellationToken cancellationToken)
    {
        await AdminClinicAuthorization.EnsureClinicStaffAsync(
                currentUserService,
                clinicStaffMembershipService,
                request.ClinicId,
                "Only hospital staff can record a mental health assessment.",
                cancellationToken)
            .ConfigureAwait(false);

        if (!MentalHealthInstruments.TryResolve(request.AssessmentType, out var instrument))
            throw new BusinessRuleException(
                $"Only {Phq9Instrument.AssessmentType} or {Gad7Instrument.AssessmentType} is supported.");

        if (!await patientClinicAccessService
                .HasClinicAccessAsync(request.PatientId, request.ClinicId, cancellationToken)
                .ConfigureAwait(false))
            throw new BusinessRuleException("Patient does not have access to this hospital.");

        var patient = await patientRepository.GetByIdAsync(request.PatientId, cancellationToken)
            .ConfigureAwait(false);
        if (patient is null || patient.IsDeleted)
            return null;

        var answersByOrder = request.Answers.ToDictionary(a => a.Order, a => a.NumericScore);
        var assessment = MentalHealthAssessmentFactory.Create(
            request.ClinicId,
            request.PatientId,
            instrument,
            answersByOrder,
            clock.UtcNow);

        await assessmentRepository.AddAsync(assessment, cancellationToken).ConfigureAwait(false);
        await unitOfWork.SaveChangesAsync(cancellationToken).ConfigureAwait(false);

        return MentalHealthAssessmentMapper.ToDetailDto(assessment);
    }
}
