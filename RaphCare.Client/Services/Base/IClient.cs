namespace RaphCare.Client.Services.Base;

/// <summary>
/// Extended IClient: exposes HttpClient and typed patient operations that communicate with the generated Client (ClientService.cs).
/// </summary>
public partial interface IClient
{
    HttpClient HttpClient { get; }

    Task<PatientDto?> GetPatientAsync(Guid id, CancellationToken cancellationToken = default);
    Task<PagedResultOfPatientDto?> GetPatientsAsync(int? pageNumber = null, int? pageSize = null, CancellationToken cancellationToken = default);
    Task<CreatePatientResult?> CreatePatientAsync(CreatePatientCommand? body = null, CancellationToken cancellationToken = default);
    Task UpdatePatientAsync(Guid id, UpdatePatientCommand? body = null, CancellationToken cancellationToken = default);
}
