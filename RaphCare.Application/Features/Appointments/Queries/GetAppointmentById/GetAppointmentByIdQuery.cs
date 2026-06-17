using MediatR;
using RaphCare.Application.Features.Appointments.DTOs;

namespace RaphCare.Application.Features.Appointments.Queries.GetAppointmentById;

public class GetAppointmentByIdQuery : IRequest<AppointmentDto>
{
    public Guid Id { get; set; }
}

