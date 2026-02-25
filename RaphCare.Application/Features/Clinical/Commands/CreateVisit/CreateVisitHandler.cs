using MediatR;
using RaphCare.Application.Common.Interfaces;
using RaphCare.Domain.Clinical;

namespace RaphCare.Application.Features.Clinical.Commands.CreateVisit;

public class CreateVisitHandler : IRequestHandler<CreateVisitCommand, Guid>
{
    private readonly IRepository<Visit> _repository;
    private readonly IUnitOfWork _unitOfWork;
    private readonly ICurrentUserService _currentUserService;
    private readonly IDateTimeProvider _dateTimeProvider;

    public CreateVisitHandler(
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

    public async Task<Guid> Handle(CreateVisitCommand request, CancellationToken cancellationToken)
    {
        var visit = new Visit
        {
            ClinicId = request.ClinicId,
            AppointmentId = request.AppointmentId,
            PatientId = request.PatientId,
            ProviderId = request.ProviderId,
            VisitStart = request.VisitStart,
            VisitType = request.VisitType,
            Status = "InProgress",
            Summary = request.Summary
        };

        await _repository.AddAsync(visit, cancellationToken);
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        return visit.Id;
    }
}

