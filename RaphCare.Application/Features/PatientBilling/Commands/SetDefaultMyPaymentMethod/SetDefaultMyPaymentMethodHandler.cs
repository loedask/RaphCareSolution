using MediatR;
using RaphCare.Application.Common.Exceptions;
using RaphCare.Application.Common.Interfaces;
using RaphCare.Domain.Billing;

namespace RaphCare.Application.Features.PatientBilling.Commands.SetDefaultMyPaymentMethod;

public sealed class SetDefaultMyPaymentMethodHandler(
    IRepository<PaymentMethod> methods,
    IUnitOfWork unitOfWork,
    ICurrentUserService currentUser) : IRequestHandler<SetDefaultMyPaymentMethodCommand, Unit>
{
    private readonly IRepository<PaymentMethod> _methods = methods;
    private readonly IUnitOfWork _unitOfWork = unitOfWork;
    private readonly ICurrentUserService _currentUser = currentUser;

    public async Task<Unit> Handle(SetDefaultMyPaymentMethodCommand request, CancellationToken cancellationToken)
    {
        var patientId = _currentUser.CurrentPatientId
            ?? throw new ForbiddenAccessException("A patient profile is required.");

        var target = await _methods.GetByIdAsync(request.PaymentMethodId, cancellationToken).ConfigureAwait(false);
        if (target is null || target.PatientId != patientId || !target.IsActive)
        {
            throw new NotFoundException(nameof(PaymentMethod), request.PaymentMethodId);
        }

        var defaults = await _methods.SearchAsync(
            q => q.Where(m => m.PatientId == patientId && m.IsActive && m.IsDefault),
            1,
            50,
            true,
            cancellationToken).ConfigureAwait(false);
        foreach (var m in defaults.Items)
        {
            if (m.Id == target.Id)
                continue;
            m.IsDefault = false;
            await _methods.UpdateAsync(m, cancellationToken).ConfigureAwait(false);
        }

        target.IsDefault = true;
        await _methods.UpdateAsync(target, cancellationToken).ConfigureAwait(false);
        await _unitOfWork.SaveChangesAsync(cancellationToken).ConfigureAwait(false);
        return Unit.Value;
    }
}
