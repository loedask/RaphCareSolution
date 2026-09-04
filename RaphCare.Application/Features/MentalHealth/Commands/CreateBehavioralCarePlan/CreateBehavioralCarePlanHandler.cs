using MediatR;
using RaphCare.Application.Common.Exceptions;
using RaphCare.Application.Common.Interfaces;
using RaphCare.Application.Features.MentalHealth.DTOs;
using RaphCare.Application.Features.Organization;
using RaphCare.Domain.MentalHealth;
using RaphCare.Domain.Patients;

namespace RaphCare.Application.Features.MentalHealth.Commands.CreateBehavioralCarePlan;

public sealed class CreateBehavioralCarePlanHandler(
    ICurrentUserService currentUserService,
    IClinicStaffMembershipService clinicStaffMembershipService,
    IPatientClinicAccessService patientClinicAccessService,
    IRepository<Patient> patientRepository,
    IRepository<BehavioralCarePlan> planRepository,
    IDateTimeProvider clock,
    IUnitOfWork unitOfWork)
    : IRequestHandler<CreateBehavioralCarePlanCommand, BehavioralCarePlanDto?>
{
    public async Task<BehavioralCarePlanDto?> Handle(
        CreateBehavioralCarePlanCommand request,
        CancellationToken cancellationToken)
    {
        await AdminClinicAuthorization.EnsureClinicStaffAsync(
                currentUserService,
                clinicStaffMembershipService,
                request.ClinicId,
                "Only hospital staff can create a behavioral care plan.",
                cancellationToken)
            .ConfigureAwait(false);

        if (!await patientClinicAccessService
                .HasClinicAccessAsync(request.PatientId, request.ClinicId, cancellationToken)
                .ConfigureAwait(false))
            throw new BusinessRuleException("Patient does not have access to this hospital.");

        var patient = await patientRepository.GetByIdAsync(request.PatientId, cancellationToken)
            .ConfigureAwait(false);
        if (patient is null || patient.IsDeleted)
            return null;

        var plan = new BehavioralCarePlan
        {
            ClinicId = request.ClinicId,
            PatientId = request.PatientId,
            Title = request.Title.Trim(),
            Description = request.Description.Trim(),
            StartDate = clock.UtcNow.Date,
            Status = "Active"
        };

        foreach (var goal in request.Goals)
        {
            plan.Goals.Add(new TherapyGoal
            {
                BehavioralCarePlanId = plan.Id,
                GoalDescription = goal.GoalDescription.Trim(),
                TargetDate = DateTime.SpecifyKind(goal.TargetDate.Date, DateTimeKind.Utc),
                IsCompleted = false
            });
        }

        await planRepository.AddAsync(plan, cancellationToken).ConfigureAwait(false);
        await unitOfWork.SaveChangesAsync(cancellationToken).ConfigureAwait(false);

        return TherapyMapper.ToCarePlanDto(plan);
    }
}
