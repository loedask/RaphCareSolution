using System.Buffers.Binary;
using MediatR;
using RaphCare.Application.Common;
using RaphCare.Application.Common.Exceptions;
using RaphCare.Application.Common.Interfaces;
using RaphCare.Application.Features.PatientTelehealth.DTOs;
using RaphCare.Domain.Organization;
using RaphCare.Domain.Telemedicine;

namespace RaphCare.Application.Features.PatientTelehealth.Queries.GetTelehealthJoinInfo;

public class GetTelehealthJoinInfoHandler : IRequestHandler<GetTelehealthJoinInfoQuery, TelehealthJoinInfoDto>
{
    private const int DefaultTtlSeconds = 3600;

    private readonly IRepository<TeleSession> _repository;
    private readonly IRepository<Provider> _providers;
    private readonly IApplicationUserStore _users;
    private readonly ICurrentUserService _currentUser;
    private readonly ITelehealthRtcTokenGenerator _rtcTokenGenerator;

    public GetTelehealthJoinInfoHandler(
        IRepository<TeleSession> repository,
        IRepository<Provider> providers,
        IApplicationUserStore users,
        ICurrentUserService currentUser,
        ITelehealthRtcTokenGenerator rtcTokenGenerator)
    {
        _repository = repository;
        _providers = providers;
        _users = users;
        _currentUser = currentUser;
        _rtcTokenGenerator = rtcTokenGenerator;
    }

    public async Task<TelehealthJoinInfoDto> Handle(GetTelehealthJoinInfoQuery request, CancellationToken cancellationToken)
    {
        var patientId = _currentUser.CurrentPatientId
            ?? throw new ForbiddenAccessException("A patient profile is required for telehealth.");

        var session = await _repository.GetByIdAsync(request.TeleSessionId, cancellationToken).ConfigureAwait(false);
        if (session is null || session.PatientId != patientId)
            throw new NotFoundException(nameof(TeleSession), request.TeleSessionId);

        var channel = ResolveChannelName(session);
        var uid = request.Uid ?? StableUidFromGuid(patientId);
        var providerDisplayName = await ResolveProviderDisplayNameAsync(session.ProviderId, cancellationToken).ConfigureAwait(false);

        var dto = new TelehealthJoinInfoDto
        {
            TeleSessionId = session.Id,
            ChannelName = channel,
            Uid = uid,
            AppId = _rtcTokenGenerator.AppId,
            Status = session.Status,
            ScheduledStart = session.ScheduledStart,
            RtcConfigured = _rtcTokenGenerator.IsConfigured,
            TokenExpiresAtUnix = DateTimeOffset.UtcNow.ToUnixTimeSeconds() + DefaultTtlSeconds,
            ProviderDisplayName = providerDisplayName,
        };

        if (_rtcTokenGenerator.IsConfigured)
        {
            dto.RtcToken = _rtcTokenGenerator.BuildRtcToken(channel, uid, DefaultTtlSeconds);
        }

        return dto;
    }

    private async Task<string?> ResolveProviderDisplayNameAsync(Guid providerId, CancellationToken cancellationToken)
    {
        if (providerId == Guid.Empty)
            return null;

        var provider = await _providers.GetByIdAsync(providerId, cancellationToken).ConfigureAwait(false);
        if (provider is null || provider.ApplicationUserId == Guid.Empty)
            return null;

        var user = await _users.FindByIdAsync(provider.ApplicationUserId, cancellationToken).ConfigureAwait(false);
        var name = user?.DisplayName?.Trim();
        return string.IsNullOrWhiteSpace(name) ? null : name;
    }

    private static string ResolveChannelName(TeleSession session)
    {
        if (!string.IsNullOrWhiteSpace(session.SessionExternalId))
            return session.SessionExternalId.Trim();
        return $"raph-tele-{session.Id:N}";
    }

    private static uint StableUidFromGuid(Guid g)
    {
        Span<byte> bytes = stackalloc byte[16];
        g.TryWriteBytes(bytes);
        var n = BinaryPrimitives.ReadUInt32LittleEndian(bytes);
        return n == 0 ? 1u : n & 0x7FFF_FFFFu;
    }
}
