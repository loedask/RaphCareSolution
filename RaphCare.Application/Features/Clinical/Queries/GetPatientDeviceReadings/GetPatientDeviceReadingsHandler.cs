using FluentValidation.Results;
using MediatR;
using Microsoft.EntityFrameworkCore;
using RaphCare.Application.Common.DTOs;
using RaphCare.Application.Common.Exceptions;
using RaphCare.Application.Common.Interfaces;
using RaphCare.Application.Features.Clinical.DTOs;
using RaphCare.Domain.Devices;

namespace RaphCare.Application.Features.Clinical.Queries.GetPatientDeviceReadings;

public sealed class GetPatientDeviceReadingsHandler(
    IRepository<DeviceReading> readings,
    IClinicContext clinicContext) : IRequestHandler<GetPatientDeviceReadingsQuery, PagedResult<PatientDeviceReadingListItemDto>>
{
    private readonly IRepository<DeviceReading> _readings = readings;
    private readonly IClinicContext _clinicContext = clinicContext;

    public async Task<PagedResult<PatientDeviceReadingListItemDto>> Handle(
        GetPatientDeviceReadingsQuery request,
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

        var readingType = string.IsNullOrWhiteSpace(request.ReadingType) ? null : request.ReadingType.Trim();

        var paged = await _readings.SearchAsync(
            q =>
            {
                q = q
                    .Include(r => r.Device)
                    .Where(r => r.PatientId == request.PatientId && r.Device.ClinicId == clinicId);

                if (readingType != null)
                    q = q.Where(r => r.ReadingType == readingType);

                if (request.RecordedFromUtc is { } from)
                    q = q.Where(r => r.RecordedAt >= from);

                if (request.RecordedToUtc is { } to)
                    q = q.Where(r => r.RecordedAt <= to);

                return q.OrderByDescending(r => r.RecordedAt);
            },
            pageNumber,
            pageSize,
            applyDefaultIdOrdering: false,
            cancellationToken).ConfigureAwait(false);

        var items = paged.Items.Select(Map).ToList();

        return new PagedResult<PatientDeviceReadingListItemDto>
        {
            Items = items,
            TotalCount = paged.TotalCount,
            PageNumber = paged.PageNumber,
            PageSize = paged.PageSize
        };
    }

    private static PatientDeviceReadingListItemDto Map(DeviceReading r)
    {
        var d = r.Device;
        var dto = new PatientDeviceReadingListItemDto
        {
            Id = r.Id,
            DeviceId = r.DeviceId,
            SerialNumber = d?.SerialNumber ?? string.Empty,
            Model = d?.Model ?? string.Empty,
            ReadingType = r.ReadingType,
            Unit = r.Unit,
            RecordedAt = r.RecordedAt,
            ReceivedAt = r.ReceivedAt,
            PrimaryValue = r.PrimaryValue
        };

        switch (r)
        {
            case HeartRateReading hr:
                dto.Kind = nameof(HeartRateReading);
                dto.HeartRateBpm = (int)hr.HeartRate;
                break;
            case PulseOximeterReading po:
                dto.Kind = nameof(PulseOximeterReading);
                dto.SpO2Percent = po.SpO2;
                if (po.PulseRate > 0)
                    dto.PulseRateBpm = (int)po.PulseRate;
                break;
            default:
                dto.Kind = r.GetType().Name;
                break;
        }

        return dto;
    }
}
