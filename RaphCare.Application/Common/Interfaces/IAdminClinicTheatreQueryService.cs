using RaphCare.Application.Features.Organization.DTOs;

namespace RaphCare.Application.Common.Interfaces;

public interface IAdminClinicTheatreQueryService
{
    Task<AdminClinicTheatreBoardDto?> GetBoardAsync(
        Guid clinicId,
        DateTime dayUtc,
        CancellationToken cancellationToken = default);
}
