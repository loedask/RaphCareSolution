using MediatR;
using RaphCare.Application.Common.Exceptions;
using RaphCare.Application.Common.Interfaces;
using RaphCare.Domain.Devices;

namespace RaphCare.Application.Features.PatientDevices.Queries.GetMyLatestReadings;

public sealed class GetMyLatestReadingsHandler : IRequestHandler<GetMyLatestReadingsQuery, GetMyLatestReadingsResponseDto>
{
    private readonly IRepository<DeviceReading> _readings;
    private readonly ICurrentUserService _currentUser;

    public GetMyLatestReadingsHandler(IRepository<DeviceReading> readings, ICurrentUserService currentUser)
    {
        _readings = readings;
        _currentUser = currentUser;
    }

    public async Task<GetMyLatestReadingsResponseDto> Handle(GetMyLatestReadingsQuery request, CancellationToken cancellationToken)
    {
        var patientId = _currentUser.CurrentPatientId
            ?? throw new ForbiddenAccessException("A patient profile is required.");

        var paged = await _readings.SearchAsync(
            q => q
                .Where(r => r.PatientId == patientId)
                .OrderByDescending(r => r.RecordedAt),
            1,
            100,
            applyDefaultIdOrdering: false,
            cancellationToken).ConfigureAwait(false);

        var dto = new GetMyLatestReadingsResponseDto();
        foreach (var reading in paged.Items)
        {
            if (string.Equals(reading.ReadingType, "HeartRate", StringComparison.OrdinalIgnoreCase)
                && dto.HeartRateRecordedAt is null)
            {
                dto.HeartRateBpm = reading.PrimaryValue;
                dto.HeartRateRecordedAt = reading.RecordedAt;
            }
            else if (string.Equals(reading.ReadingType, "SpO2", StringComparison.OrdinalIgnoreCase)
                     && dto.SpO2RecordedAt is null)
            {
                dto.SpO2Percent = reading.PrimaryValue;
                dto.SpO2RecordedAt = reading.RecordedAt;
            }

            if (dto.HeartRateRecordedAt is not null && dto.SpO2RecordedAt is not null)
                break;
        }

        return dto;
    }
}
