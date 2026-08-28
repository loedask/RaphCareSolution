using MediatR;
using RaphCare.Application.Common.Exceptions;
using RaphCare.Application.Common.Interfaces;
using RaphCare.Application.Features.PatientTelehealth.DTOs;
using RaphCare.Domain.Telemedicine;

namespace RaphCare.Application.Features.PatientTelehealth.Queries.GetTelehealthSessionChat;

public sealed class GetTelehealthSessionChatHandler(
    IRepository<TeleSession> teleSessions,
    IRepository<TeleSessionChat> chat,
    ICurrentUserService currentUser) : IRequestHandler<GetTelehealthSessionChatQuery, IReadOnlyList<TelehealthChatMessageDto>>
{
    public async Task<IReadOnlyList<TelehealthChatMessageDto>> Handle(
        GetTelehealthSessionChatQuery request,
        CancellationToken cancellationToken)
    {
        var patientId = currentUser.CurrentPatientId
            ?? throw new ForbiddenAccessException("A patient profile is required.");

        var userId = currentUser.CurrentUserId
            ?? throw new ForbiddenAccessException("Sign in required.");

        var session = await teleSessions.GetByIdAsync(request.TeleSessionId, cancellationToken).ConfigureAwait(false);
        if (session is null || session.PatientId != patientId)
            throw new NotFoundException(nameof(TeleSession), request.TeleSessionId);

        var pageSize = Math.Clamp(request.PageSize, 1, 200);
        var page = await chat.SearchAsync(
            q => q.Where(c => c.TeleSessionId == request.TeleSessionId).OrderBy(c => c.SentAt),
            pageNumber: 1,
            pageSize: pageSize,
            applyDefaultIdOrdering: false,
            cancellationToken: cancellationToken).ConfigureAwait(false);

        return page.Items
            .Select(c => new TelehealthChatMessageDto
            {
                Id = c.Id,
                Message = c.Message,
                SentAt = c.SentAt,
                IsMine = c.SenderUserId == userId,
                IsSystemMessage = c.IsSystemMessage
            })
            .ToList();
    }
}
