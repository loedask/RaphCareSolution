using RaphCare.Client.Contracts;
using RaphCare.Client.Models;

namespace RaphCare.Client.Contracts.Interfaces;

public interface IAdminClinicService
{
    Task<Response<IReadOnlyList<ClinicListItem>>> GetClinicsAsync(CancellationToken cancellationToken = default);
    Task<Response<RegisterClinicResult>> RegisterClinicAsync(RegisterClinicRequest request, CancellationToken cancellationToken = default);
}
