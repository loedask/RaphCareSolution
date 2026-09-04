using MediatR;
using Microsoft.EntityFrameworkCore;
using RaphCare.Application.Common.Interfaces;
using RaphCare.Application.Features.MentalHealth.DTOs;
using RaphCare.Application.Features.Organization;
using RaphCare.Domain.MentalHealth;

namespace RaphCare.Application.Features.MentalHealth.Commands.CompleteTherapyGoal;

public sealed class CompleteTherapyGoalHandler(
    ICurrentUserService currentUserService,
    IClinicStaffMembershipService clinicStaffMembershipService,
    IRepository<BehavioralCarePlan> planRepository,
    IUnitOfWork unitOfWork)
    : IRequestHandler<CompleteTherapyGoalCommand, TherapyGoalDto?>
{
    public async Task<TherapyGoalDto?> Handle(
        CompleteTherapyGoalCommand request,
        CancellationToken cancellationToken)
    {
        await AdminClinicAuthorization.EnsureClinicStaffAsync(
                currentUserService,
                clinicStaffMembershipService,
                request.ClinicId,
                "Only hospital staff can update therapy goals.",
                cancellationToken)
            .ConfigureAwait(false);

        var page = await planRepository.SearchAsync(
                q => q
                    .Where(p => p.Id == request.CarePlanId && p.ClinicId == request.ClinicId)
                    .Include(p => p.Goals),
                1,
                1,
                applyDefaultIdOrdering: false,
                cancellationToken)
            .ConfigureAwait(false);

        var plan = page.Items.Count > 0 ? page.Items[0] : null;
        var goal = plan?.Goals.FirstOrDefault(g => g.Id == request.GoalId);
        if (goal is null)
            return null;

        goal.IsCompleted = request.IsCompleted;
        await planRepository.UpdateAsync(plan!, cancellationToken).ConfigureAwait(false);
        await unitOfWork.SaveChangesAsync(cancellationToken).ConfigureAwait(false);

        return new TherapyGoalDto
        {
            Id = goal.Id,
            GoalDescription = goal.GoalDescription,
            TargetDate = goal.TargetDate,
            IsCompleted = goal.IsCompleted
        };
    }
}
