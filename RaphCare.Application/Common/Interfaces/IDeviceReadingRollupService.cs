using RaphCare.Application.Features.Clinical.DTOs;

namespace RaphCare.Application.Common.Interfaces;

/// <summary>Server-side aggregates for device vitals (avoids paging raw rows for charts).</summary>
public interface IDeviceReadingRollupService
{
    /// <summary>One row per UTC date with samples in <paramref name="fromUtc"/>..<paramref name="toUtc"/> (inclusive).</summary>
    Task<IReadOnlyList<DeviceReadingDailyRollupDto>> GetDailyRollupsAsync(
        Guid patientId,
        Guid clinicId,
        DateTime fromUtc,
        DateTime toUtc,
        CancellationToken cancellationToken = default);
}
