using RaphCare.Client.Contracts;
using RaphCare.Client.Models.Patients;

namespace RaphCare.Client.Services;

public interface IPatientService
{
    Task<Response<PatientViewModel?>> GetByIdAsync(Guid id, CancellationToken cancellationToken = default);
    Task<Response<PagedResultViewModel<PatientViewModel>>> GetListAsync(int pageNumber = 1, int pageSize = 20, CancellationToken cancellationToken = default);
    Task<Response<Guid>> CreateAsync(CreatePatientRequest request, CancellationToken cancellationToken = default);
    Task<Response<bool>> UpdateAsync(Guid id, UpdatePatientRequest request, CancellationToken cancellationToken = default);
}
