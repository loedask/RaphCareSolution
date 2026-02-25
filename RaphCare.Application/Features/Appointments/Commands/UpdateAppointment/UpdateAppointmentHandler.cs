using MediatR;
using RaphCare.Application.Common.Exceptions;
using RaphCare.Application.Common.Interfaces;
using RaphCare.Domain.Clinical;

namespace RaphCare.Application.Features.Appointments.Commands.UpdateAppointment;

public class UpdateAppointmentHandler : IRequestHandler<UpdateAppointmentCommand, Unit>
{
    private readonly IRepository<Appointment> _repository;
    private readonly IUnitOfWork _unitOfWork;
    private readonly ICurrentUserService _currentUserService;
    private readonly IDateTimeProvider _dateTimeProvider;

    public UpdateAppointmentHandler(
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

    public async Task<Unit> Handle(UpdateAppointmentCommand request, CancellationToken cancellationToken)
    {
        var appointment = await _repository.GetByIdAsync(request.Id, cancellationToken);
        if (appointment is null)
        {
            throw new NotFoundException(nameof(Appointment), request.Id);
        }

        if (request.ScheduledStart.HasValue)
        {
            appointment.ScheduledStart = request.ScheduledStart.Value;
        }

        if (request.ScheduledEnd.HasValue)
        {
            appointment.ScheduledEnd = request.ScheduledEnd.Value;
        }

        if (!string.IsNullOrWhiteSpace(request.Status))
        {
            appointment.Status = request.Status;
        }

        if (!string.IsNullOrWhiteSpace(request.Reason))
        {
            appointment.Reason = request.Reason;
        }

        await _repository.UpdateAsync(appointment, cancellationToken);
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        return Unit.Value;
    }
}

