using MediatR;
using RaphCare.Application.Common.DTOs;
using RaphCare.Application.Common.Interfaces;
using RaphCare.Application.Features.Appointments.DTOs;
using RaphCare.Domain.Clinical;

namespace RaphCare.Application.Features.Appointments.Queries.GetAppointments;

public class GetAppointmentsHandler : IRequestHandler<GetAppointmentsQuery, PagedResult<AppointmentDto>>
{
    private readonly IRepository<Appointment> _repository;

    public GetAppointmentsHandler(IRepository<Appointment> repository)
    {
        _repository = repository;
    }

    public async Task<PagedResult<AppointmentDto>> Handle(GetAppointmentsQuery request, CancellationToken cancellationToken)
    {
        var appointments = await _repository.ListAsync(cancellationToken);

        var totalCount = appointments.Count;

        var items = appointments
            .Skip((request.PageNumber - 1) * request.PageSize)
            .Take(request.PageSize)
            .Select(a => new AppointmentDto
            {
                Id = a.Id,
                ClinicId = a.ClinicId,
                PatientId = a.PatientId,
                ProviderId = a.ProviderId,
                ScheduledStart = a.ScheduledStart,
                ScheduledEnd = a.ScheduledEnd,
                Status = a.Status,
                Type = a.Type,
                Reason = a.Reason
            })
            .ToList();

        return new PagedResult<AppointmentDto>
        {
            Items = items,
            TotalCount = totalCount,
            PageNumber = request.PageNumber,
            PageSize = request.PageSize
        };
    }
}

