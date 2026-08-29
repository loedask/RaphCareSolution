using RaphCare.Application.Features.Organization.DTOs;

namespace RaphCare.Application.Common.Interfaces;

public interface IAdminClinicCollectionQueryService
{
    Task<AdminClinicCollectionBoardDto?> GetPendingOrdersAsync(
        Guid clinicId,
        string? search,
        CancellationToken cancellationToken = default);

    Task<CollectionDisplayBoardDto?> GetDisplayBoardAsync(
        string token,
        CancellationToken cancellationToken = default);
}
