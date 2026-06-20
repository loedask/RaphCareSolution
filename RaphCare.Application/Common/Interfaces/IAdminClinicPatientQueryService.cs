using RaphCare.Application.Common.DTOs;
using RaphCare.Application.Features.Organization.DTOs;

namespace RaphCare.Application.Common.Interfaces;

public interface IAdminClinicPatientQueryService
{
    Task<PagedResult<AdminClinicPatientListItemDto>> GetPatientsAsync(
        Guid clinicId,
        int pageNumber,
        int pageSize,
        CancellationToken cancellationToken = default);
}
