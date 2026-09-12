using MediatR;
using RaphCare.Application.Common.Exceptions;
using RaphCare.Application.Common.Interfaces;
using RaphCare.Application.Features.Appointments;
using RaphCare.Domain.Clinical;
using RaphCare.Domain.Organization;

namespace RaphCare.Application.Features.Appointments.Commands.CreatePatientAppointment;

public class CreatePatientAppointmentHandler : IRequestHandler<CreatePatientAppointmentCommand, Guid>
{
    private readonly IRepository<Appointment> _repository;
    private readonly IRepository<AppointmentReminder> _reminderRepository;
    private readonly IRepository<Clinic> _clinicRepository;
    private readonly IUnitOfWork _unitOfWork;
    private readonly ICurrentUserService _currentUser;
    private readonly IDateTimeProvider _clock;
    private readonly IMediator _mediator;

    public CreatePatientAppointmentHandler(
        IRepository<Appointment> repository,
        IRepository<AppointmentReminder> reminderRepository,
        IRepository<Clinic> clinicRepository,
        IUnitOfWork unitOfWork,
        ICurrentUserService currentUser,
        IDateTimeProvider clock,
        IMediator mediator)
    {
        _repository = repository;
        _reminderRepository = reminderRepository;
        _clinicRepository = clinicRepository;
        _unitOfWork = unitOfWork;
        _currentUser = currentUser;
        _clock = clock;
        _mediator = mediator;
    }

    public async Task<Guid> Handle(CreatePatientAppointmentCommand request, CancellationToken cancellationToken)
    {
        var patientId = _currentUser.CurrentPatientId
            ?? throw new ForbiddenAccessException("A patient profile is required to book appointments.");

        var appointment = new Appointment
        {
            ClinicId = request.ClinicId,
            PatientId = patientId,
            ProviderId = request.ProviderId,
            ScheduledStart = request.ScheduledStart,
            ScheduledEnd = request.ScheduledEnd,
            Type = request.Type,
            Status = "Scheduled",
            Reason = string.IsNullOrWhiteSpace(request.Reason) ? null : request.Reason.Trim()
        };

        await _repository.AddAsync(appointment, cancellationToken).ConfigureAwait(false);
        await AppointmentReminderPlanner.ReplaceUnsentAsync(
                _reminderRepository,
                appointment.Id,
                appointment.ScheduledStart,
                _clock.UtcNow,
                cancellationToken)
            .ConfigureAwait(false);
        await _unitOfWork.SaveChangesAsync(cancellationToken).ConfigureAwait(false);

        var clinic = await _clinicRepository.GetByIdAsync(request.ClinicId, cancellationToken).ConfigureAwait(false);
        await AppointmentPatientNotifier.NotifyBookedAsync(
                _mediator,
                patientId,
                clinic?.Name ?? string.Empty,
                appointment.ScheduledStart,
                cancellationToken)
            .ConfigureAwait(false);

        return appointment.Id;
    }
}
