using MediatR;
using RaphCare.Application.Common.Exceptions;
using RaphCare.Application.Common.Interfaces;
using RaphCare.Application.Features.Appointments.DTOs;
using RaphCare.Domain.Clinical;

namespace RaphCare.Application.Features.Appointments.Queries.GetAppointmentById;

public class GetAppointmentByIdHandler : IRequestHandler<GetAppointmentByIdQuery, AppointmentDto>
{
    private readonly IRepository<Appointment> _repository;

    public GetAppointmentByIdHandler(IRepository<Appointment> repository)
    {
        _repository = repository;
    }

    public async Task<AppointmentDto> Handle(GetAppointmentByIdQuery request, CancellationToken cancellationToken)
    {
        var appointment = await _repository.GetByIdAsync(request.Id, cancellationToken);
        if (appointment is null)
        {
            throw new NotFoundException(nameof(Appointment), request.Id);
        }

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

