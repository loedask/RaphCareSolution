using MediatR;
using RaphCare.Application.Common.Exceptions;
using RaphCare.Application.Common.Interfaces;
using RaphCare.Domain.Billing;

namespace RaphCare.Application.Features.PatientBilling.Commands.RemoveMyPaymentMethod;

public sealed class RemoveMyPaymentMethodHandler(
    IRepository<PaymentMethod> methods,
    IUnitOfWork unitOfWork,
    ICurrentUserService currentUser) : IRequestHandler<RemoveMyPaymentMethodCommand, Unit>
{
    private readonly IRepository<PaymentMethod> _methods = methods;
    private readonly IUnitOfWork _unitOfWork = unitOfWork;
    private readonly ICurrentUserService _currentUser = currentUser;

    public async Task<Unit> Handle(RemoveMyPaymentMethodCommand request, CancellationToken cancellationToken)
    {
        var patientId = _currentUser.CurrentPatientId
            ?? throw new ForbiddenAccessException("A patient profile is required.");

        var entity = await _methods.GetByIdAsync(request.PaymentMethodId, cancellationToken).ConfigureAwait(false);
        if (entity is null || entity.PatientId != patientId || !entity.IsActive)
        {
            throw new NotFoundException(nameof(PaymentMethod), request.PaymentMethodId);
        }

        var wasDefault = entity.IsDefault;
        entity.IsActive = false;
        entity.IsDefault = false;
        await _methods.UpdateAsync(entity, cancellationToken).ConfigureAwait(false);

        if (wasDefault)
        {
            var others = await _methods.SearchAsync(
                q => q.Where(m => m.PatientId == patientId && m.IsActive).OrderBy(m => m.CreatedAt),
                1,
                50,
                applyDefaultIdOrdering: false,
                cancellationToken).ConfigureAwait(false);
            var replacement = others.Items.Count > 0 ? others.Items[0] : null;
            if (replacement is not null)
            {
                replacement.IsDefault = true;
                await _methods.UpdateAsync(replacement, cancellationToken).ConfigureAwait(false);
            }
        }

        await _unitOfWork.SaveChangesAsync(cancellationToken).ConfigureAwait(false);
        return Unit.Value;
    }
}
