using FluentValidation.Results;
using MediatR;
using Microsoft.EntityFrameworkCore;
using RaphCare.Application.Common.Exceptions;
using RaphCare.Application.Common.Interfaces;
using RaphCare.Application.Features.Interoperability.DTOs;
using RaphCare.Domain.Devices;

namespace RaphCare.Application.Features.Interoperability.Queries.GetFhirObservations;

public sealed class GetFhirObservationsHandler(
    IRepository<DeviceReading> readings,
    IClinicContext clinicContext,
    IDeviceReadingFhirMapper mapper) : IRequestHandler<GetFhirObservationsQuery, FhirBundleDto>
{
    private readonly IRepository<DeviceReading> _readings = readings;
    private readonly IClinicContext _clinicContext = clinicContext;
    private readonly IDeviceReadingFhirMapper _mapper = mapper;

    public async Task<FhirBundleDto> Handle(GetFhirObservationsQuery request, CancellationToken cancellationToken)
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
        var pageSize = request.PageSize < 1 ? 20 : Math.Min(request.PageSize, 100);
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

        var entries = new List<FhirBundleEntryDto>();
        foreach (var row in paged.Items)
        {
            if (row.Device is null)
                continue;

            var obs = await _mapper.MapToObservationAsync(row, row.Device, cancellationToken).ConfigureAwait(false);
            entries.Add(new FhirBundleEntryDto
            {
                FullUrl = $"urn:raphcare:fhir:Observation/{row.Id}",
                Resource = obs
            });
        }

        return new FhirBundleDto
        {
            Total = paged.TotalCount,
            Entry = entries
        };
    }
}
