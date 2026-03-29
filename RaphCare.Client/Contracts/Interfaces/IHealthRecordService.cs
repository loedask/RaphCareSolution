using RaphCare.Client.Contracts;
using RaphCare.Client.Models.HealthRecords;

namespace RaphCare.Client.Contracts.Interfaces;

/// <summary>Patient health records API (<c>api/patient/health-records</c>) via generated <see cref="Services.Base.IClient"/>.</summary>
public interface IHealthRecordService
{
    Task<Response<PagedHealthRecordsViewModel>> GetMyHealthRecordsAsync(int pageNumber = 1, int pageSize = 20, CancellationToken cancellationToken = default);
    Task<Response<HealthRecordDetailViewModel?>> GetMyHealthRecordAsync(Guid id, CancellationToken cancellationToken = default);
}
