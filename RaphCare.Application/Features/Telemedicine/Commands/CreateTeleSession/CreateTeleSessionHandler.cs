using MediatR;
using RaphCare.Application.Common.Interfaces;
using RaphCare.Domain.Telemedicine;

namespace RaphCare.Application.Features.Telemedicine.Commands.CreateTeleSession;

public class CreateTeleSessionHandler : IRequestHandler<CreateTeleSessionCommand, Guid>
{
    private readonly IRepository<TeleSession> _repository;
    private readonly IUnitOfWork _unitOfWork;
    private readonly ICurrentUserService _currentUserService;
    private readonly IDateTimeProvider _dateTimeProvider;

    public CreateTeleSessionHandler(
        IRepository<TeleSession> repository,
        IUnitOfWork unitOfWork,
        ICurrentUserService currentUserService,
        IDateTimeProvider dateTimeProvider)
    {
        _repository = repository;
        _unitOfWork = unitOfWork;
        _currentUserService = currentUserService;
        _dateTimeProvider = dateTimeProvider;
    }

    public async Task<Guid> Handle(CreateTeleSessionCommand request, CancellationToken cancellationToken)
    {
        var session = new TeleSession
        {
            ClinicId = request.ClinicId,
            AppointmentId = request.AppointmentId,
            VisitId = request.VisitId,
            PatientId = request.PatientId,
            ProviderId = request.ProviderId,
            ScheduledStart = request.ScheduledStart,
            Status = "Scheduled",
            Platform = request.Platform,
            IsSecure = true
        };

        await _repository.AddAsync(session, cancellationToken);
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        return session.Id;
    }
}

