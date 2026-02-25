using MediatR;
using RaphCare.Application.Common.Interfaces;
using RaphCare.Domain.Clinical;

namespace RaphCare.Application.Features.Appointments.Commands.CreateAppointment;

public class CreateAppointmentHandler : IRequestHandler<CreateAppointmentCommand, Guid>
{
    private readonly IRepository<Appointment> _repository;
    private readonly IUnitOfWork _unitOfWork;
    private readonly ICurrentUserService _currentUserService;
    private readonly IDateTimeProvider _dateTimeProvider;

    public CreateAppointmentHandler(
        IRepository<Appointment> repository,
        IUnitOfWork unitOfWork,
        ICurrentUserService currentUserService,
        IDateTimeProvider dateTimeProvider)
    {
        _repository = repository;
        _unitOfWork = unitOfWork;
        _currentUserService = currentUserService;
        _dateTimeProvider = dateTimeProvider;
    }

    public async Task<Guid> Handle(CreateAppointmentCommand request, CancellationToken cancellationToken)
    {
        var appointment = new Appointment
        {
            ClinicId = request.ClinicId,
            PatientId = request.PatientId,
            ProviderId = request.ProviderId,
            ScheduledStart = request.ScheduledStart,
            ScheduledEnd = request.ScheduledEnd,
            Type = request.Type,
            Status = "Scheduled",
            Reason = request.Reason
        };

        await _repository.AddAsync(appointment, cancellationToken);
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        return appointment.Id;
    }
}

