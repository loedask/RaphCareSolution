using MediatR;
using RaphCare.Application.Common.Exceptions;
using RaphCare.Application.Common.Interfaces;
using RaphCare.Application.Features.Appointments.DTOs;
using RaphCare.Domain.Clinical;

namespace RaphCare.Application.Features.Appointments.Queries.GetMyAppointmentById;

public class GetMyAppointmentByIdHandler : IRequestHandler<GetMyAppointmentByIdQuery, AppointmentDto>
{
    private readonly IRepository<Appointment> _repository;
    private readonly ICurrentUserService _currentUser;

    public GetMyAppointmentByIdHandler(IRepository<Appointment> repository, ICurrentUserService currentUser)
    {
        _repository = repository;
        _currentUser = currentUser;
    }

    public async Task<AppointmentDto> Handle(GetMyAppointmentByIdQuery request, CancellationToken cancellationToken)
    {
        var patientId = _currentUser.CurrentPatientId
            ?? throw new ForbiddenAccessException("A patient profile is required to view appointments.");

        var appointment = await _repository.GetByIdAsync(request.Id, cancellationToken).ConfigureAwait(false);
        if (appointment is null || appointment.PatientId != patientId)
            throw new NotFoundException(nameof(Appointment), request.Id);

        return new AppointmentDto
        {
            Id = appointment.Id,
            ClinicId = appointment.ClinicId,
            PatientId = appointment.PatientId,
            ProviderId = appointment.ProviderId,
            ScheduledStart = appointment.ScheduledStart,
            ScheduledEnd = appointment.ScheduledEnd,
            Status = appointment.Status,
            Type = appointment.Type,
            Reason = appointment.Reason
        };
    }
}
