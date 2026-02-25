using MediatR;
using RaphCare.Application.Common.Interfaces;
using RaphCare.Domain.Billing;

namespace RaphCare.Application.Features.Billing.Commands.CreateInvoice;

public class CreateInvoiceHandler : IRequestHandler<CreateInvoiceCommand, Guid>
{
    private readonly IRepository<Invoice> _repository;
    private readonly IUnitOfWork _unitOfWork;
    private readonly ICurrentUserService _currentUserService;
    private readonly IDateTimeProvider _dateTimeProvider;

    public CreateInvoiceHandler(
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

    public async Task<Guid> Handle(CreateInvoiceCommand request, CancellationToken cancellationToken)
    {
        var invoice = new Invoice
        {
            ClinicId = request.ClinicId,
            PatientId = request.PatientId,
            VisitId = request.VisitId,
            Amount = request.Amount,
            Currency = request.Currency,
            DueDate = request.DueDate,
            Status = "Pending"
        };

        await _repository.AddAsync(invoice, cancellationToken);
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        return invoice.Id;
    }
}
