using MediatR;
using RaphCare.Application.Features.Appointments.DTOs;

namespace RaphCare.Application.Features.Appointments.Queries.GetMyAppointmentById;

public class GetMyAppointmentByIdQuery : IRequest<AppointmentDto>
{
    public Guid Id { get; set; }
}
