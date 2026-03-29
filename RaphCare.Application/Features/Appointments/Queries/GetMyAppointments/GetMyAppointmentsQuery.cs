using MediatR;
using RaphCare.Application.Common.DTOs;
using RaphCare.Application.Features.Appointments.DTOs;

namespace RaphCare.Application.Features.Appointments.Queries.GetMyAppointments;

public class GetMyAppointmentsQuery : IRequest<PagedResult<AppointmentDto>>
{
    public int PageNumber { get; set; } = 1;
    public int PageSize { get; set; } = 20;
}
