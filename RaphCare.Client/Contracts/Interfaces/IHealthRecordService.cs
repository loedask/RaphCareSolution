using RaphCare.Client.Contracts;
using RaphCare.Client.Models.HealthRecords;

namespace RaphCare.Client.Contracts.Interfaces;

/// <summary>Patient health records and collection orders (<c>api/patient/health-records</c>, <c>api/patient/collection-orders</c>).</summary>
public interface IHealthRecordService
{
    Task<Response<PagedHealthRecordsViewModel>> GetMyHealthRecordsAsync(int pageNumber = 1, int pageSize = 20, CancellationToken cancellationToken = default);
    Task<Response<HealthRecordDetailViewModel?>> GetMyHealthRecordAsync(Guid id, CancellationToken cancellationToken = default);
    Task<Response<PatientCollectionOrdersViewModel>> GetMyCollectionOrdersAsync(CancellationToken cancellationToken = default);
}
