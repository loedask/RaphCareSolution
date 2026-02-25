using MediatR;
using RaphCare.Application.Common.Exceptions;
using RaphCare.Application.Common.Interfaces;
using RaphCare.Domain.Clinical;

namespace RaphCare.Application.Features.Clinical.Commands.UpdateVisit;

public class UpdateVisitHandler : IRequestHandler<UpdateVisitCommand>
{
    private readonly IRepository<Visit> _repository;
    private readonly IUnitOfWork _unitOfWork;
    private readonly ICurrentUserService _currentUserService;
    private readonly IDateTimeProvider _dateTimeProvider;

    public UpdateVisitHandler(
        IRepository<Visit> repository,
        IUnitOfWork unitOfWork,
        ICurrentUserService currentUserService,
        IDateTimeProvider dateTimeProvider)
    {
        _repository = repository;
        _unitOfWork = unitOfWork;
        _currentUserService = currentUserService;
        _dateTimeProvider = dateTimeProvider;
    }

    public async Task<Unit> Handle(UpdateVisitCommand request, CancellationToken cancellationToken)
    {
        var visit = await _repository.GetByIdAsync(request.Id, cancellationToken);
        if (visit is null)
        {
            throw new NotFoundException(nameof(Visit), request.Id);
        }

        if (request.VisitEnd.HasValue)
        {
            visit.VisitEnd = request.VisitEnd.Value;
        }

        if (!string.IsNullOrWhiteSpace(request.Status))
        {
            visit.Status = request.Status;
        }

        if (!string.IsNullOrWhiteSpace(request.Summary))
        {
            visit.Summary = request.Summary;
        }

        await _repository.UpdateAsync(visit, cancellationToken);
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        return Unit.Value;
    }
}

