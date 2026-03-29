using RaphCare.Client.Contracts;
using RaphCare.Client.Contracts.Interfaces;
using RaphCare.Client.Models.HealthRecords;
using RaphCare.Client.Services.Base;

namespace RaphCare.Client.Services;

public class HealthRecordService(IClient client, HttpClient httpClient) : BaseHttpService(client, httpClient), IHealthRecordService
{
    public Task<Response<PagedHealthRecordsViewModel>> GetMyHealthRecordsAsync(int pageNumber = 1, int pageSize = 20, CancellationToken cancellationToken = default) =>
        GetAsync<PagedHealthRecordsViewModel>($"api/patient/health-records?pageNumber={pageNumber}&pageSize={pageSize}", cancellationToken);

    public Task<Response<HealthRecordDetailViewModel?>> GetMyHealthRecordAsync(Guid id, CancellationToken cancellationToken = default) =>
        GetAsync<HealthRecordDetailViewModel?>($"api/patient/health-records/{id}", cancellationToken);
}
