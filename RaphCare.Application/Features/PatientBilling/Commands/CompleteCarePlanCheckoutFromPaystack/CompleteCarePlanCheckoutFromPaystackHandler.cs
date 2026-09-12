using MediatR;
using RaphCare.Application.Common.Interfaces;
using RaphCare.Application.Features.PatientBilling;
using RaphCare.Application.Features.PatientBilling.Commands.ConfirmCarePlanCheckout;
using RaphCare.Domain.Billing;

namespace RaphCare.Application.Features.PatientBilling.Commands.CompleteCarePlanCheckoutFromPaystack;

public sealed class CompleteCarePlanCheckoutFromPaystackHandler(
    IPaymentGatewayService paymentGateway,
    IRepository<PatientCarePlan> plans,
    IUnitOfWork unitOfWork,
    IDateTimeProvider clock) : IRequestHandler<CompleteCarePlanCheckoutFromPaystackCommand, Unit>
{
    public async Task<Unit> Handle(
        CompleteCarePlanCheckoutFromPaystackCommand request,
        CancellationToken cancellationToken)
    {
        if (string.IsNullOrWhiteSpace(request.Reference))
            return Unit.Value;

        var verified = await paymentGateway.VerifyCheckoutAsync(request.Reference.Trim(), cancellationToken)
            .ConfigureAwait(false);

        if (!verified.Succeeded || verified.PatientId is not Guid patientId)
            return Unit.Value;

        var target = PatientBillingCatalog.FindByCode(verified.PlanCode);
        if (target is null || target.Tier <= 0)
            return Unit.Value;

        await ConfirmCarePlanCheckoutHandler.UpsertCarePlanAsync(
                plans,
                unitOfWork,
                clock,
                patientId,
                target.PlanCode,
                target.DisplayName,
                target.Tier,
                cancellationToken)
            .ConfigureAwait(false);

        return Unit.Value;
    }
}
