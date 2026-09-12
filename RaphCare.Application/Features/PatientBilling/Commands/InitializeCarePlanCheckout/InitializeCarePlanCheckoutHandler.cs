using FluentValidation.Results;
using MediatR;
using RaphCare.Application.Common.Exceptions;
using RaphCare.Application.Common.Interfaces;
using RaphCare.Application.Features.PatientBilling;
using RaphCare.Application.Features.PatientBilling.DTOs;
using ValidationException = RaphCare.Application.Common.Exceptions.ValidationException;

namespace RaphCare.Application.Features.PatientBilling.Commands.InitializeCarePlanCheckout;

public sealed class InitializeCarePlanCheckoutHandler(
    IPaymentGatewayService paymentGateway,
    ICurrentUserService currentUser,
    IApplicationUserStore userStore) : IRequestHandler<InitializeCarePlanCheckoutCommand, CarePlanCheckoutDto>
{
    public async Task<CarePlanCheckoutDto> Handle(
        InitializeCarePlanCheckoutCommand request,
        CancellationToken cancellationToken)
    {
        var patientId = currentUser.CurrentPatientId
            ?? throw new ForbiddenAccessException("A patient profile is required.");

        if (!paymentGateway.IsConfigured)
        {
            throw new ValidationException(
            [
                new ValidationFailure(
                    "Paystack",
                    "Card checkout is not configured. Set Paystack:SecretKey or use Free without payment.")
            ]);
        }

        var target = PatientBillingCatalog.FindByCode(request.PlanCode);
        if (target is null || target.Tier <= 0)
        {
            throw new ValidationException(
            [
                new ValidationFailure(nameof(request.PlanCode), "Choose Essential Care or Complete Care to pay.")
            ]);
        }

        var email = "patient@raphcare.local";
        if (currentUser.CurrentUserId is Guid userId)
        {
            var user = await userStore.FindByIdAsync(userId, cancellationToken).ConfigureAwait(false);
            if (!string.IsNullOrWhiteSpace(user?.Email) && user.Email.Contains('@', StringComparison.Ordinal))
                email = user.Email.Trim();
        }

        var session = await paymentGateway.InitializeCheckoutAsync(
                patientId,
                email,
                target.MonthlyPrice,
                target.Currency,
                target.PlanCode,
                request.CallbackUrl,
                cancellationToken)
            .ConfigureAwait(false);

        if (session is null)
        {
            throw new ValidationException(
            [
                new ValidationFailure("Paystack", "Could not start checkout.")
            ]);
        }

        return new CarePlanCheckoutDto
        {
            Reference = session.Reference,
            AuthorizationUrl = session.AuthorizationUrl,
            AccessCode = session.AccessCode,
            PlanCode = target.PlanCode,
            Amount = target.MonthlyPrice,
            Currency = target.Currency
        };
    }
}
