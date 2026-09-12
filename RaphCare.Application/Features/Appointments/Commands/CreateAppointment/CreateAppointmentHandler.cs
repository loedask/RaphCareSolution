using MediatR;
using RaphCare.Application.Common.Interfaces;
using RaphCare.Application.Features.Appointments;
using RaphCare.Domain.Clinical;
using RaphCare.Domain.Organization;

namespace RaphCare.Application.Features.Appointments.Commands.CreateAppointment;

public class CreateAppointmentHandler : IRequestHandler<CreateAppointmentCommand, Guid>
{
    private readonly IRepository<Appointment> _repository;
    private readonly IRepository<AppointmentReminder> _reminderRepository;
    private readonly IRepository<Clinic> _clinicRepository;
    private readonly IUnitOfWork _unitOfWork;
    private readonly ICurrentUserService _currentUserService;
    private readonly IDateTimeProvider _dateTimeProvider;
    private readonly IMediator _mediator;

    public CreateAppointmentHandler(
        IRepository<Appointment> repository,
        IRepository<AppointmentReminder> reminderRepository,
        IRepository<Clinic> clinicRepository,
        IUnitOfWork unitOfWork,
        ICurrentUserService currentUserService,
        IDateTimeProvider dateTimeProvider,
        IMediator mediator)
    {
        _repository = repository;
        _reminderRepository = reminderRepository;
        _clinicRepository = clinicRepository;
        _unitOfWork = unitOfWork;
        _currentUserService = currentUserService;
        _dateTimeProvider = dateTimeProvider;
        _mediator = mediator;
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

        await _repository.AddAsync(appointment, cancellationToken).ConfigureAwait(false);
        await AppointmentReminderPlanner.ReplaceUnsentAsync(
                _reminderRepository,
                appointment.Id,
                appointment.ScheduledStart,
                _dateTimeProvider.UtcNow,
                cancellationToken)
            .ConfigureAwait(false);
        await _unitOfWork.SaveChangesAsync(cancellationToken).ConfigureAwait(false);

        var clinic = await _clinicRepository.GetByIdAsync(request.ClinicId, cancellationToken).ConfigureAwait(false);
        await AppointmentPatientNotifier.NotifyBookedAsync(
                _mediator,
                request.PatientId,
                clinic?.Name ?? string.Empty,
                appointment.ScheduledStart,
                cancellationToken)
            .ConfigureAwait(false);

        return appointment.Id;
    }
}
