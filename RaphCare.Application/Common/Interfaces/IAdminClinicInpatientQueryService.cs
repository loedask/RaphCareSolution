using RaphCare.Application.Common.DTOs;
using RaphCare.Application.Features.Organization.DTOs;

namespace RaphCare.Application.Common.Interfaces;

public interface IAdminClinicInpatientQueryService
{
    Task<AdminClinicInpatientBoardDto?> GetBoardAsync(Guid clinicId, CancellationToken cancellationToken = default);

    Task<PagedResult<AdminClinicAdmissionDto>?> GetAdmissionsAsync(
        Guid clinicId,
        int pageNumber,
        int pageSize,
        string? status = null,
        CancellationToken cancellationToken = default);

    Task<AdminClinicAdmissionDto?> GetAdmissionByIdAsync(
        Guid clinicId,
        Guid admissionId,
        CancellationToken cancellationToken = default);
}
