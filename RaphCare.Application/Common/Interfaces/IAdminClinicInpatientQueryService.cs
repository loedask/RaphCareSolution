using RaphCare.Application.Features.Organization.DTOs;

namespace RaphCare.Application.Common.Interfaces;

public interface IAdminClinicInpatientQueryService
{
    Task<AdminClinicInpatientBoardDto?> GetBoardAsync(Guid clinicId, CancellationToken cancellationToken = default);
}
