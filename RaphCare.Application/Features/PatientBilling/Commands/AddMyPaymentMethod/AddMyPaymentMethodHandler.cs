using MediatR;
using RaphCare.Application.Common.Exceptions;
using RaphCare.Application.Common.Interfaces;
using RaphCare.Domain.Billing;

namespace RaphCare.Application.Features.PatientBilling.Commands.AddMyPaymentMethod;

public sealed class AddMyPaymentMethodHandler(
    IRepository<PaymentMethod> methods,
    IUnitOfWork unitOfWork,
    ICurrentUserService currentUser) : IRequestHandler<AddMyPaymentMethodCommand, Guid>
{
    private readonly IRepository<PaymentMethod> _methods = methods;
    private readonly IUnitOfWork _unitOfWork = unitOfWork;
    private readonly ICurrentUserService _currentUser = currentUser;

    public async Task<Guid> Handle(AddMyPaymentMethodCommand request, CancellationToken cancellationToken)
    {
        var patientId = _currentUser.CurrentPatientId
            ?? throw new ForbiddenAccessException("A patient profile is required.");

        if (request.SetAsDefault)
        {
            var existing = await _methods.SearchAsync(
                q => q.Where(m => m.PatientId == patientId && m.IsActive && m.IsDefault),
                1,
                50,
                true,
                cancellationToken).ConfigureAwait(false);
            foreach (var m in existing.Items)
            {
                m.IsDefault = false;
                await _methods.UpdateAsync(m, cancellationToken).ConfigureAwait(false);
            }
        }
        else
        {
            var any = await _methods.SearchAsync(
                q => q.Where(m => m.PatientId == patientId && m.IsActive),
                1,
                1,
                true,
                cancellationToken).ConfigureAwait(false);
            if (any.TotalCount == 0)
                request.SetAsDefault = true;
        }

        var entity = new PaymentMethod
        {
            PatientId = patientId,
            MethodType = request.MethodType.Trim(),
            ProviderName = request.ProviderName.Trim(),
            MaskedDetails = request.MaskedDetails.Trim(),
            IsDefault = request.SetAsDefault,
            IsActive = true
        };

        await _methods.AddAsync(entity, cancellationToken).ConfigureAwait(false);
        await _unitOfWork.SaveChangesAsync(cancellationToken).ConfigureAwait(false);
        return entity.Id;
    }
}
