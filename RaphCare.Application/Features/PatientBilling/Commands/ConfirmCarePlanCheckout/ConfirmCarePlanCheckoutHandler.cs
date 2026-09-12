using FluentValidation.Results;
using MediatR;
using RaphCare.Application.Common.Exceptions;
using RaphCare.Application.Common.Interfaces;
using RaphCare.Application.Features.PatientBilling;
using RaphCare.Domain.Billing;
using ValidationException = RaphCare.Application.Common.Exceptions.ValidationException;

namespace RaphCare.Application.Features.PatientBilling.Commands.ConfirmCarePlanCheckout;

public sealed class ConfirmCarePlanCheckoutHandler(
    IPaymentGatewayService paymentGateway,
    IRepository<PatientCarePlan> plans,
    IUnitOfWork unitOfWork,
    ICurrentUserService currentUser,
    IDateTimeProvider clock) : IRequestHandler<ConfirmCarePlanCheckoutCommand, Unit>
{
    public async Task<Unit> Handle(ConfirmCarePlanCheckoutCommand request, CancellationToken cancellationToken)
    {
        if (string.IsNullOrWhiteSpace(request.Reference))
        {
            throw new ValidationException(
            [
                new ValidationFailure(nameof(request.Reference), "Payment reference is required.")
            ]);
        }

        var patientId = currentUser.CurrentPatientId
            ?? throw new ForbiddenAccessException("A patient profile is required.");

        var verified = await paymentGateway.VerifyCheckoutAsync(request.Reference.Trim(), cancellationToken)
            .ConfigureAwait(false);

        if (!verified.Succeeded)
        {
            throw new ValidationException(
            [
                new ValidationFailure(nameof(request.Reference), verified.GatewayMessage ?? "Payment was not successful.")
            ]);
        }

        if (verified.PatientId is Guid paidPatient && paidPatient != patientId)
            throw new ForbiddenAccessException("This payment belongs to another patient.");

        var planCode = verified.PlanCode ?? string.Empty;
        var target = PatientBillingCatalog.FindByCode(planCode);
        if (target is null || target.Tier <= 0)
        {
            throw new ValidationException(
            [
                new ValidationFailure("PlanCode", "Paid plan could not be resolved from the payment.")
            ]);
        }

        await UpsertCarePlanAsync(plans, unitOfWork, clock, patientId, target.PlanCode, target.DisplayName, target.Tier, cancellationToken)
            .ConfigureAwait(false);
        return Unit.Value;
    }

    internal static async Task UpsertCarePlanAsync(
        IRepository<PatientCarePlan> plans,
        IUnitOfWork unitOfWork,
        IDateTimeProvider clock,
        Guid patientId,
        string planCode,
        string displayName,
        int tier,
        CancellationToken cancellationToken)
    {
        var existing = await plans.SearchAsync(
            q => q.Where(p => p.PatientId == patientId),
            1,
            1,
            true,
            cancellationToken).ConfigureAwait(false);

        var now = clock.UtcNow;
        var renews = now.AddMonths(1).Date;

        if (existing.Items.Count == 0)
        {
            await plans.AddAsync(new PatientCarePlan
            {
                PatientId = patientId,
                PlanCode = planCode,
                PlanDisplayName = displayName,
                Tier = tier,
                EffectiveFrom = now,
                RenewsOn = renews,
                Status = "Active"
            }, cancellationToken).ConfigureAwait(false);
        }
        else
        {
            var row = existing.Items[0];
            row.PlanCode = planCode;
            row.PlanDisplayName = displayName;
            row.Tier = tier;
            row.EffectiveFrom = now;
            row.RenewsOn = renews;
            row.Status = "Active";
            await plans.UpdateAsync(row, cancellationToken).ConfigureAwait(false);
        }

        await unitOfWork.SaveChangesAsync(cancellationToken).ConfigureAwait(false);
    }
}
