using Microsoft.EntityFrameworkCore;
using RaphCare.Application.Common.Interfaces;
using RaphCare.Application.Features.Clinical.DTOs;
using RaphCare.Domain.Devices;

namespace RaphCare.Persistence;

/// <summary>EF-backed daily rollups over <see cref="HeartRateReading"/> and <see cref="PulseOximeterReading"/> for the device database.</summary>
public sealed class DeviceReadingRollupService(DeviceDbContext db) : IDeviceReadingRollupService
{
    private readonly DeviceDbContext _db = db ?? throw new ArgumentNullException(nameof(db));

    public async Task<IReadOnlyList<DeviceReadingDailyRollupDto>> GetDailyRollupsAsync(
        Guid patientId,
        Guid clinicId,
        DateTime fromUtc,
        DateTime toUtc,
        CancellationToken cancellationToken = default)
    {
        var from = DateTime.SpecifyKind(fromUtc, DateTimeKind.Utc);
        var to = DateTime.SpecifyKind(toUtc, DateTimeKind.Utc);

        var hrRows = await _db.Set<HeartRateReading>().AsNoTracking()
            .Join(
                _db.Set<Device>().AsNoTracking(),
                r => r.DeviceId,
                d => d.Id,
                (r, d) => new { r, d })
            .Where(x => x.r.PatientId == patientId
                        && x.d.ClinicId == clinicId
                        && x.r.RecordedAt >= from
                        && x.r.RecordedAt <= to)
            .GroupBy(x => x.r.RecordedAt.Date)
            .Select(g => new
            {
                Date = g.Key,
                Count = g.Count(),
                Avg = g.Average(x => x.r.HeartRate),
                Min = g.Min(x => x.r.HeartRate),
                Max = g.Max(x => x.r.HeartRate)
            })
            .ToListAsync(cancellationToken).ConfigureAwait(false);

        var spo2Rows = await _db.Set<PulseOximeterReading>().AsNoTracking()
            .Join(
                _db.Set<Device>().AsNoTracking(),
                r => r.DeviceId,
                d => d.Id,
                (r, d) => new { r, d })
            .Where(x => x.r.PatientId == patientId
                        && x.d.ClinicId == clinicId
                        && x.r.RecordedAt >= from
                        && x.r.RecordedAt <= to)
            .GroupBy(x => x.r.RecordedAt.Date)
            .Select(g => new
            {
                Date = g.Key,
                Count = g.Count(),
                Avg = g.Average(x => x.r.SpO2),
                Min = g.Min(x => x.r.SpO2),
                Max = g.Max(x => x.r.SpO2)
            })
            .ToListAsync(cancellationToken).ConfigureAwait(false);

        var dates = hrRows.Select(x => x.Date).Concat(spo2Rows.Select(x => x.Date)).Distinct().OrderBy(d => d).ToList();

        var list = new List<DeviceReadingDailyRollupDto>(dates.Count);
        foreach (var date in dates)
        {
            var hr = hrRows.FirstOrDefault(x => x.Date == date);
            var sp = spo2Rows.FirstOrDefault(x => x.Date == date);
            list.Add(new DeviceReadingDailyRollupDto
            {
                Date = DateOnly.FromDateTime(date),
                HeartRateSampleCount = hr?.Count ?? 0,
                AvgHeartRateBpm = hr != null ? decimal.Round((decimal)hr.Avg, 1, MidpointRounding.AwayFromZero) : null,
                MinHeartRateBpm = hr != null ? (int)Math.Round(hr.Min, MidpointRounding.AwayFromZero) : null,
                MaxHeartRateBpm = hr != null ? (int)Math.Round(hr.Max, MidpointRounding.AwayFromZero) : null,
                SpO2SampleCount = sp?.Count ?? 0,
                AvgSpO2Percent = sp != null ? decimal.Round((decimal)sp.Avg, 1, MidpointRounding.AwayFromZero) : null,
                MinSpO2Percent = sp != null ? sp.Min : null,
                MaxSpO2Percent = sp != null ? sp.Max : null
            });
        }

        return list;
    }
}
