using RaphCare.Application.Features.Organization.DTOs;

namespace RaphCare.Application.Common.Interfaces;

public interface IAdminClinicCasualtyQueryService
{
    Task<AdminClinicCasualtyBoardDto?> GetBoardAsync(
        Guid clinicId,
        CancellationToken cancellationToken = default);

    Task<CasualtyDisplayBoardDto?> GetDisplayBoardAsync(
        string token,
        CancellationToken cancellationToken = default);
}
