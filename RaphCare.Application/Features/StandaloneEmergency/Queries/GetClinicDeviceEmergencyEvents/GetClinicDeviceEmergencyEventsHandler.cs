using MediatR;
using Microsoft.EntityFrameworkCore;
using RaphCare.Application.Common.DTOs;
using RaphCare.Application.Common.Exceptions;
using RaphCare.Application.Common.Interfaces;
using RaphCare.Application.Features.StandaloneEmergency.DTOs;
using RaphCare.Domain.Devices;
using RaphCare.Domain.Patients;

namespace RaphCare.Application.Features.StandaloneEmergency.Queries.GetClinicDeviceEmergencyEvents;

public sealed class GetClinicDeviceEmergencyEventsHandler(
    IRepository<DeviceEmergencyEvent> events,
    IRepository<Patient> patients,
    IClinicContext clinicContext) : IRequestHandler<GetClinicDeviceEmergencyEventsQuery, PagedResult<ClinicDeviceEmergencyEventListItemDto>>
{
    private readonly IRepository<DeviceEmergencyEvent> _events = events;
    private readonly IRepository<Patient> _patients = patients;
    private readonly IClinicContext _clinicContext = clinicContext;

    public async Task<PagedResult<ClinicDeviceEmergencyEventListItemDto>> Handle(
        GetClinicDeviceEmergencyEventsQuery request,
        CancellationToken cancellationToken)
    {
        var clinicId = _clinicContext.ClinicId
            ?? throw new ForbiddenAccessException("Missing clinic context.");

        var pageNumber = request.PageNumber < 1 ? 1 : request.PageNumber;
        var pageSize = request.PageSize < 1 ? 20 : Math.Min(request.PageSize, 200);

        var paged = await _events.SearchAsync(
            q =>
            {
                q = q
                    .Include(e => e.Device)
                    .Where(e => e.ClinicId == clinicId);

                if (request.OccurredFromUtc is { } from)
                    q = q.Where(e => e.OccurredAtUtc >= from);

                if (request.OccurredToUtc is { } to)
                    q = q.Where(e => e.OccurredAtUtc <= to);

                return q.OrderByDescending(e => e.OccurredAtUtc);
            },
            pageNumber,
            pageSize,
            applyDefaultIdOrdering: false,
            cancellationToken).ConfigureAwait(false);

        var patientIds = paged.Items.Select(e => e.PatientId).Distinct().ToList();
        var names = new Dictionary<Guid, string>();
        foreach (var patientId in patientIds)
        {
            var patient = await _patients.GetByIdAsync(patientId, cancellationToken).ConfigureAwait(false);
            if (patient is not null)
                names[patientId] = $"{patient.FirstName} {patient.LastName}".Trim();
        }

        var items = paged.Items.Select(e => new ClinicDeviceEmergencyEventListItemDto
        {
            Id = e.Id,
            PatientId = e.PatientId,
            PatientName = names.TryGetValue(e.PatientId, out var name) && !string.IsNullOrWhiteSpace(name)
                ? name
                : "Patient",
            DeviceId = e.DeviceId,
            SerialNumber = e.Device?.SerialNumber ?? string.Empty,
            Model = e.Device?.Model ?? string.Empty,
            EventType = e.EventType,
            OccurredAtUtc = e.OccurredAtUtc,
            ReceivedAtUtc = e.ReceivedAtUtc,
            Latitude = e.Latitude,
            Longitude = e.Longitude,
            HorizontalAccuracyMeters = e.HorizontalAccuracyMeters,
            ExternalCorrelationId = e.ExternalCorrelationId,
            CaregiversNotified = e.CaregiversNotified,
            CaregiversNotifiedAtUtc = e.CaregiversNotifiedAtUtc,
            CaregiverNotificationSummary = e.CaregiverNotificationSummary
        }).ToList();

        return new PagedResult<ClinicDeviceEmergencyEventListItemDto>
        {
            Items = items,
            TotalCount = paged.TotalCount,
            PageNumber = paged.PageNumber,
            PageSize = paged.PageSize
        };
    }
}
