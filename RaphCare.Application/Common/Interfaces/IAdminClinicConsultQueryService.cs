using RaphCare.Application.Features.Organization.DTOs;

namespace RaphCare.Application.Common.Interfaces;

public interface IAdminClinicConsultQueryService
{
    Task<AdminClinicConsultBoardDto?> GetBoardAsync(
        Guid clinicId,
        CancellationToken cancellationToken = default);

    Task<ConsultDisplayBoardDto?> GetDisplayBoardAsync(
        string token,
        CancellationToken cancellationToken = default);
}
