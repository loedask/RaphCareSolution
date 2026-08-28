using RaphCare.Application.Common.DTOs;
using RaphCare.Application.Features.Organization.DTOs;

namespace RaphCare.Application.Common.Interfaces;

public interface IAdminClinicPatientQueryService
{
    Task<PagedResult<AdminClinicPatientListItemDto>> GetPatientsAsync(
        Guid clinicId,
        int pageNumber,
        int pageSize,
        string? search = null,
        CancellationToken cancellationToken = default);

    Task<AdminClinicPatientDetailDto?> GetPatientDetailAsync(
        Guid clinicId,
        Guid patientId,
        CancellationToken cancellationToken = default);

    Task<AdminClinicVisitClinicalDocumentationDto> GetVisitClinicalDocumentationAsync(
        Guid visitId,
        DateTime visitStart,
        CancellationToken cancellationToken = default);
}
