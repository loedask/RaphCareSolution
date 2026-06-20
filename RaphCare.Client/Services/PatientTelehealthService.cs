using RaphCare.Client.Contracts;
using RaphCare.Client.Contracts.Interfaces;
using RaphCare.Client.Models.Api;
using RaphCare.Client.Models.Telehealth;
using RaphCare.Client.Services.Base;

namespace RaphCare.Client.Services;

public sealed class PatientTelehealthService(HttpClient httpClient) : BaseHttpService(httpClient), IPatientTelehealthService
{
    public async Task<Response<PagedPatientTeleSessionsViewModel>> GetMySessionsAsync(
        int pageNumber = 1,
        int pageSize = 20,
        CancellationToken cancellationToken = default)
    {
        var result = await GetAsync<PagedApiResult<TeleSessionListItemDto>>(
                $"api/patient/telehealth/sessions?pageNumber={pageNumber}&pageSize={pageSize}",
                cancellationToken)
            .ConfigureAwait(false);

        if (!result.IsSuccess || result.Data is null)
            return Response<PagedPatientTeleSessionsViewModel>.Failure(result.ErrorMessage ?? "Could not load sessions.", result.StatusCode);

        var paged = result.Data;
        return Response<PagedPatientTeleSessionsViewModel>.Success(new PagedPatientTeleSessionsViewModel
        {
            Items = (paged.Items ?? Array.Empty<TeleSessionListItemDto>())
                .Select(s => new PatientTeleSessionListItemViewModel
                {
                    Id = s.Id,
                    ScheduledStart = s.ScheduledStart,
                    Status = s.Status ?? string.Empty,
                    Platform = s.Platform ?? string.Empty
                })
                .ToList(),
            TotalCount = paged.TotalCount,
            PageNumber = paged.PageNumber,
            PageSize = paged.PageSize
        });
    }

    public async Task<Response<TelehealthJoinInfoViewModel?>> GetJoinInfoAsync(
        Guid teleSessionId,
        int? uid = null,
        CancellationToken cancellationToken = default)
    {
        var url = uid.HasValue
            ? $"api/patient/telehealth/sessions/{teleSessionId}/join-info?uid={uid.Value}"
            : $"api/patient/telehealth/sessions/{teleSessionId}/join-info";

        var result = await GetAsync<JoinInfoDto>(url, cancellationToken).ConfigureAwait(false);
        if (!result.IsSuccess)
            return Response<TelehealthJoinInfoViewModel?>.Failure(result.ErrorMessage ?? "Could not load join info.", result.StatusCode);
        if (result.Data is null)
            return Response<TelehealthJoinInfoViewModel?>.Success(null);

        var d = result.Data;
        return Response<TelehealthJoinInfoViewModel?>.Success(new TelehealthJoinInfoViewModel
        {
            TeleSessionId = d.TeleSessionId,
            ChannelName = d.ChannelName ?? string.Empty,
            Uid = unchecked((uint)d.Uid),
            AppId = d.AppId,
            RtcToken = d.RtcToken,
            TokenExpiresAtUnix = d.TokenExpiresAtUnix,
            RtcConfigured = d.RtcConfigured,
            Status = d.Status ?? string.Empty,
            ScheduledStart = d.ScheduledStart
        });
    }

    public Task<Response<bool>> SendSessionSmsAsync(Guid teleSessionId, CancellationToken cancellationToken = default) =>
        PostNoContentAsync($"api/patient/telehealth/sessions/{teleSessionId}/notify-sms", body: null, cancellationToken);

    private sealed class TeleSessionListItemDto
    {
        public Guid Id { get; set; }
        public DateTime ScheduledStart { get; set; }
        public string? Status { get; set; }
        public string? Platform { get; set; }
    }

    private sealed class JoinInfoDto
    {
        public Guid TeleSessionId { get; set; }
        public string? ChannelName { get; set; }
        public int Uid { get; set; }
        public string? AppId { get; set; }
        public string? RtcToken { get; set; }
        public long TokenExpiresAtUnix { get; set; }
        public bool RtcConfigured { get; set; }
        public string? Status { get; set; }
        public DateTime ScheduledStart { get; set; }
    }
}
