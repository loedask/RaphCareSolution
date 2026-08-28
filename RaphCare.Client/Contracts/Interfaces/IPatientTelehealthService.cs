using RaphCare.Client.Contracts;
using RaphCare.Client.Models.Telehealth;

namespace RaphCare.Client.Contracts.Interfaces;

/// <summary>Patient telehealth API (<c>api/patient/telehealth/...</c>).</summary>
public interface IPatientTelehealthService
{
    Task<Response<PagedPatientTeleSessionsViewModel>> GetMySessionsAsync(int pageNumber = 1, int pageSize = 20, CancellationToken cancellationToken = default);
    Task<Response<TelehealthJoinInfoViewModel?>> GetJoinInfoAsync(Guid teleSessionId, int? uid = null, CancellationToken cancellationToken = default);
    Task<Response<bool>> SendSessionSmsAsync(Guid teleSessionId, CancellationToken cancellationToken = default);
    Task<Response<Guid>> RequestOnDemandSessionAsync(
        Guid? clinicId = null,
        Guid? providerId = null,
        string? callMode = null,
        CancellationToken cancellationToken = default);
    Task<Response<IReadOnlyList<TelehealthChatMessageViewModel>>> GetSessionChatAsync(
        Guid teleSessionId,
        int pageSize = 50,
        CancellationToken cancellationToken = default);
    Task<Response<Guid>> SendChatMessageAsync(
        Guid teleSessionId,
        string message,
        CancellationToken cancellationToken = default);
}
