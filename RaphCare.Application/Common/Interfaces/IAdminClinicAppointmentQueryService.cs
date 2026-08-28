using RaphCare.Application.Common.DTOs;
using RaphCare.Application.Features.Organization.DTOs;

namespace RaphCare.Application.Common.Interfaces;

public interface IAdminClinicAppointmentQueryService
{
    Task<PagedResult<AdminClinicAppointmentListItemDto>> GetAppointmentsAsync(
        Guid clinicId,
        int pageNumber,
        int pageSize,
        DateTime? fromUtc = null,
        DateTime? toUtc = null,
        string? status = null,
        CancellationToken cancellationToken = default);
}
