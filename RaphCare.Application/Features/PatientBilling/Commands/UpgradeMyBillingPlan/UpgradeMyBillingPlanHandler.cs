using FluentValidation.Results;
using MediatR;
using RaphCare.Application.Common.Exceptions;
using RaphCare.Application.Common.Interfaces;
using RaphCare.Application.Features.PatientBilling;
using RaphCare.Domain.Billing;
using ValidationException = RaphCare.Application.Common.Exceptions.ValidationException;

namespace RaphCare.Application.Features.PatientBilling.Commands.UpgradeMyBillingPlan;

public sealed class UpgradeMyBillingPlanHandler(
    IRepository<PatientCarePlan> plans,
    IUnitOfWork unitOfWork,
    ICurrentUserService currentUser,
    IDateTimeProvider clock) : IRequestHandler<UpgradeMyBillingPlanCommand, Unit>
{
    private readonly IRepository<PatientCarePlan> _plans = plans;
    private readonly IUnitOfWork _unitOfWork = unitOfWork;
    private readonly ICurrentUserService _currentUser = currentUser;
    private readonly IDateTimeProvider _clock = clock;

    public async Task<Unit> Handle(UpgradeMyBillingPlanCommand request, CancellationToken cancellationToken)
    {
        var patientId = _currentUser.CurrentPatientId
            ?? throw new ForbiddenAccessException("A patient profile is required.");

        var target = PatientBillingCatalog.FindByCode(request.PlanCode);
        if (target is null)
        {
            throw new ValidationException(new[]
            {
                new ValidationFailure(nameof(request.PlanCode), "Unknown plan code.")
            });
        }

        var existing = await _plans.SearchAsync(
            q => q.Where(p => p.PatientId == patientId),
            1,
            1,
            true,
            cancellationToken).ConfigureAwait(false);

        if (target.Tier == 0)
        {
            foreach (var row in existing.Items)
                await _plans.DeleteAsync(row, cancellationToken).ConfigureAwait(false);
            await _unitOfWork.SaveChangesAsync(cancellationToken).ConfigureAwait(false);
            return Unit.Value;
        }

        var now = _clock.UtcNow;
        var renews = now.AddMonths(1).Date;

        if (existing.Items.Count == 0)
        {
            var row = new PatientCarePlan
            {
                PatientId = patientId,
                PlanCode = target.PlanCode,
                PlanDisplayName = target.DisplayName,
                Tier = target.Tier,
                EffectiveFrom = now,
                RenewsOn = renews,
                Status = "Active"
            };
            await _plans.AddAsync(row, cancellationToken).ConfigureAwait(false);
        }
        else
        {
            var row = existing.Items[0];
            row.PlanCode = target.PlanCode;
            row.PlanDisplayName = target.DisplayName;
            row.Tier = target.Tier;
            row.EffectiveFrom = now;
            row.RenewsOn = renews;
            row.Status = "Active";
            await _plans.UpdateAsync(row, cancellationToken).ConfigureAwait(false);
        }

        await _unitOfWork.SaveChangesAsync(cancellationToken).ConfigureAwait(false);
        return Unit.Value;
    }
}
