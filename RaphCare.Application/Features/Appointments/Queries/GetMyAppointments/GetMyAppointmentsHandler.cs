using System.Linq;
using MediatR;
using RaphCare.Application.Common.DTOs;
using RaphCare.Application.Common.Exceptions;
using RaphCare.Application.Common.Interfaces;
using RaphCare.Application.Features.Appointments.DTOs;
using RaphCare.Domain.Clinical;

namespace RaphCare.Application.Features.Appointments.Queries.GetMyAppointments;

public class GetMyAppointmentsHandler : IRequestHandler<GetMyAppointmentsQuery, PagedResult<AppointmentDto>>
{
    private readonly IRepository<Appointment> _repository;
    private readonly ICurrentUserService _currentUser;

    public GetMyAppointmentsHandler(IRepository<Appointment> repository, ICurrentUserService currentUser)
    {
        _repository = repository;
        _currentUser = currentUser;
    }

    public async Task<PagedResult<AppointmentDto>> Handle(GetMyAppointmentsQuery request, CancellationToken cancellationToken)
    {
        var patientId = _currentUser.CurrentPatientId
            ?? throw new ForbiddenAccessException("A patient profile is required to view appointments.");

        var paged = await _repository.SearchAsync(
            q => q.Where(a => a.PatientId == patientId).OrderByDescending(a => a.ScheduledStart),
            request.PageNumber,
            request.PageSize,
            applyDefaultIdOrdering: false,
            cancellationToken).ConfigureAwait(false);

        var items = paged.Items.Select(a => new AppointmentDto
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
        }).ToList();

        return new PagedResult<AppointmentDto>
        {
            Items = items,
            TotalCount = paged.TotalCount,
            PageNumber = paged.PageNumber,
            PageSize = paged.PageSize
        };
    }
}
