using RaphCare.Client.Contracts;
using RaphCare.Client.Models.Clinical;

namespace RaphCare.Client.Contracts.Interfaces;

/// <summary>Provider/clinic wearable vitals and standalone emergency events. Requires staff bearer token.</summary>
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

    Task<Response<PagedPatientDeviceEmergencyEventsViewModel>> GetEmergencyEventsAsync(
        Guid patientId,
        int pageNumber = 1,
        int pageSize = 20,
        DateTime? occurredFromUtc = null,
        DateTime? occurredToUtc = null,
        CancellationToken cancellationToken = default);

    /// <summary>Recent SOS/fall events for the current clinic (<c>X-Clinic-Id</c>).</summary>
    Task<Response<PagedClinicDeviceEmergencyEventsViewModel>> GetClinicEmergencyEventsAsync(
        int pageNumber = 1,
        int pageSize = 20,
        DateTime? occurredFromUtc = null,
        DateTime? occurredToUtc = null,
        CancellationToken cancellationToken = default);
}
