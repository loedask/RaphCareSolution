using RaphCare.Application.Features.Organization.DTOs;

namespace RaphCare.Application.Common.Interfaces;

public interface IAdminClinicProviderQueryService
{
    Task<IReadOnlyList<AdminClinicProviderListItemDto>> GetProvidersAsync(
        Guid clinicId,
        CancellationToken cancellationToken = default);

    Task<AdminClinicProviderDetailDto?> GetProviderDetailAsync(
        Guid clinicId,
        Guid providerId,
        CancellationToken cancellationToken = default);
}
