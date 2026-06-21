using RaphCare.Application.Features.Organization.DTOs;

namespace RaphCare.Application.Common.Interfaces;

public interface IAdminClinicDashboardQueryService
{
    Task<AdminClinicDashboardDto?> GetDashboardAsync(
        Guid clinicId,
        CancellationToken cancellationToken = default);
}
