using System.Buffers.Binary;
using MediatR;
using RaphCare.Application.Common.Exceptions;
using RaphCare.Application.Common.Interfaces;
using RaphCare.Application.Features.PatientTelehealth.DTOs;
using RaphCare.Domain.Telemedicine;

namespace RaphCare.Application.Features.PatientTelehealth.Queries.GetTelehealthJoinInfo;

public class GetTelehealthJoinInfoHandler : IRequestHandler<GetTelehealthJoinInfoQuery, TelehealthJoinInfoDto>
{
    private const int DefaultTtlSeconds = 3600;

    private readonly IRepository<TeleSession> _repository;
    private readonly ICurrentUserService _currentUser;
    private readonly ITelehealthRtcTokenGenerator _rtcTokenGenerator;

    public GetTelehealthJoinInfoHandler(
        IRepository<TeleSession> repository,
        ICurrentUserService currentUser,
        ITelehealthRtcTokenGenerator rtcTokenGenerator)
    {
        _repository = repository;
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

        var dto = new TelehealthJoinInfoDto
        {
            TeleSessionId = session.Id,
            ChannelName = channel,
            Uid = uid,
            AppId = _rtcTokenGenerator.AppId,
            Status = session.Status,
            ScheduledStart = session.ScheduledStart,
            RtcConfigured = _rtcTokenGenerator.IsConfigured,
            TokenExpiresAtUnix = DateTimeOffset.UtcNow.ToUnixTimeSeconds() + DefaultTtlSeconds
        };

        if (_rtcTokenGenerator.IsConfigured)
        {
            dto.RtcToken = _rtcTokenGenerator.BuildRtcToken(channel, uid, DefaultTtlSeconds);
        }

        return dto;
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
