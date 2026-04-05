using FluentValidation.Results;
using MediatR;
using Microsoft.EntityFrameworkCore;
using RaphCare.Application.Common.DTOs;
using RaphCare.Application.Common.Exceptions;
using RaphCare.Application.Common.Interfaces;
using RaphCare.Application.Features.StandaloneEmergency.DTOs;
using RaphCare.Domain.Devices;

namespace RaphCare.Application.Features.StandaloneEmergency.Queries.GetPatientDeviceEmergencyEvents;

public sealed class GetPatientDeviceEmergencyEventsHandler(
    IRepository<DeviceEmergencyEvent> events,
    IClinicContext clinicContext) : IRequestHandler<GetPatientDeviceEmergencyEventsQuery, PagedResult<PatientDeviceEmergencyEventListItemDto>>
{
    private readonly IRepository<DeviceEmergencyEvent> _events = events;
    private readonly IClinicContext _clinicContext = clinicContext;

    public async Task<PagedResult<PatientDeviceEmergencyEventListItemDto>> Handle(
        GetPatientDeviceEmergencyEventsQuery request,
        CancellationToken cancellationToken)
    {
        var clinicId = _clinicContext.ClinicId
            ?? throw new ForbiddenAccessException("Missing clinic context.");

        if (request.PatientId == Guid.Empty)
        {
            throw new ValidationException(new[]
            {
                new ValidationFailure(nameof(request.PatientId), "Patient is required.")
            });
        }

        var pageNumber = request.PageNumber < 1 ? 1 : request.PageNumber;
        var pageSize = request.PageSize < 1 ? 20 : Math.Min(request.PageSize, 200);

        var paged = await _events.SearchAsync(
            q =>
            {
                q = q
                    .Include(e => e.Device)
                    .Where(e => e.PatientId == request.PatientId && e.ClinicId == clinicId);

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

        var items = paged.Items.Select(e => new PatientDeviceEmergencyEventListItemDto
        {
            Id = e.Id,
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

        return new PagedResult<PatientDeviceEmergencyEventListItemDto>
        {
            Items = items,
            TotalCount = paged.TotalCount,
            PageNumber = paged.PageNumber,
            PageSize = paged.PageSize
        };
    }
}
