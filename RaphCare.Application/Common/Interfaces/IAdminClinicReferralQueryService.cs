using RaphCare.Application.Features.Organization.DTOs;

namespace RaphCare.Application.Common.Interfaces;

public interface IAdminClinicReferralQueryService
{
    Task<AdminClinicReferralBoardDto?> GetBoardAsync(
        Guid clinicId,
        CancellationToken cancellationToken = default);
}
