using RaphCare.Client.Contracts;
using RaphCare.Client.Models.Clinical;

namespace RaphCare.Client.Contracts.Interfaces;

/// <summary>Provider/clinic wearable vitals (<c>api/clinical/patients/{{id}}/device-readings</c> and daily rollup). Requires staff bearer token.</summary>
public interface IClinicalPatientDeviceReadingsService
{
    Task<Response<PagedPatientDeviceReadingsViewModel>> GetReadingsAsync(
        Guid patientId,
        int pageNumber = 1,
        int pageSize = 20,
        string? readingType = null,
        DateTime? recordedFromUtc = null,
        DateTime? recordedToUtc = null,
        CancellationToken cancellationToken = default);

    Task<Response<IReadOnlyList<DeviceReadingDailyRollupViewModel>>> GetDailyRollupAsync(
        Guid patientId,
        DateTime fromUtc,
        DateTime toUtc,
        CancellationToken cancellationToken = default);
}
