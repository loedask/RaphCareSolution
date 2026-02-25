using MediatR;
using RaphCare.Application.Common.Exceptions;
using RaphCare.Application.Common.Interfaces;
using RaphCare.Domain.Billing;

namespace RaphCare.Application.Features.Billing.Commands.UpdateInvoice;

public class UpdateInvoiceHandler : IRequestHandler<UpdateInvoiceCommand>
{
    private readonly IRepository<Invoice> _repository;
    private readonly IUnitOfWork _unitOfWork;
    private readonly ICurrentUserService _currentUserService;
    private readonly IDateTimeProvider _dateTimeProvider;

    public UpdateInvoiceHandler(
        IRepository<Invoice> repository,
        IUnitOfWork unitOfWork,
        ICurrentUserService currentUserService,
        IDateTimeProvider dateTimeProvider)
    {
        _repository = repository;
        _unitOfWork = unitOfWork;
        _currentUserService = currentUserService;
        _dateTimeProvider = dateTimeProvider;
    }

    public async Task<Unit> Handle(UpdateInvoiceCommand request, CancellationToken cancellationToken)
    {
        var invoice = await _repository.GetByIdAsync(request.Id, cancellationToken);
        if (invoice is null)
        {
            throw new NotFoundException(nameof(Invoice), request.Id);
        }

        if (!string.IsNullOrWhiteSpace(request.Status))
        {
            invoice.Status = request.Status;
        }

        if (request.PaidAt.HasValue)
        {
            invoice.PaidAt = request.PaidAt.Value;
        }

        await _repository.UpdateAsync(invoice, cancellationToken);
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        return Unit.Value;
    }
}
