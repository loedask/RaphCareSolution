using MediatR;
using RaphCare.Application.Common.Exceptions;
using RaphCare.Application.Common.Interfaces;
using RaphCare.Application.Features.MentalHealth;
using RaphCare.Application.Features.MentalHealth.DTOs;
using RaphCare.Domain.MentalHealth;

namespace RaphCare.Application.Features.PatientMentalHealth.Commands.SubmitMyPatientMentalHealthAssessment;

public sealed class SubmitMyPatientMentalHealthAssessmentHandler(
    ICurrentUserService currentUser,
    IPatientClinicAccessService patientClinicAccessService,
    IRepository<MentalHealthAssessment> assessmentRepository,
    IDateTimeProvider clock,
    IUnitOfWork unitOfWork)
    : IRequestHandler<SubmitMyPatientMentalHealthAssessmentCommand, MentalHealthAssessmentDetailDto?>
{
    public async Task<MentalHealthAssessmentDetailDto?> Handle(
        SubmitMyPatientMentalHealthAssessmentCommand request,
        CancellationToken cancellationToken)
    {
        var patientId = currentUser.CurrentPatientId
            ?? throw new ForbiddenAccessException("A patient profile is required.");

        if (!MentalHealthInstruments.TryResolve(request.AssessmentType, out var instrument))
            throw new BusinessRuleException(
                $"Only {Phq9Instrument.AssessmentType} or {Gad7Instrument.AssessmentType} is supported.");

        if (!await patientClinicAccessService
                .HasClinicAccessAsync(patientId, request.ClinicId, cancellationToken)
                .ConfigureAwait(false))
            throw new BusinessRuleException("You do not have access to this hospital.");

        var answersByOrder = request.Answers.ToDictionary(a => a.Order, a => a.NumericScore);
        var assessment = MentalHealthAssessmentFactory.Create(
            request.ClinicId,
            patientId,
            instrument,
            answersByOrder,
            clock.UtcNow);

        await assessmentRepository.AddAsync(assessment, cancellationToken).ConfigureAwait(false);
        await unitOfWork.SaveChangesAsync(cancellationToken).ConfigureAwait(false);

        return MentalHealthAssessmentMapper.ToDetailDto(assessment);
    }
}
