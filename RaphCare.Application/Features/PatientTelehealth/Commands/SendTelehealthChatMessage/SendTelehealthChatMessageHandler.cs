using MediatR;
using RaphCare.Application.Common.Exceptions;
using RaphCare.Application.Common.Interfaces;
using RaphCare.Domain.Telemedicine;

namespace RaphCare.Application.Features.PatientTelehealth.Commands.SendTelehealthChatMessage;

public sealed class SendTelehealthChatMessageHandler(
    IRepository<TeleSession> teleSessions,
    IRepository<TeleSessionChat> chat,
    ICurrentUserService currentUser,
    IDateTimeProvider clock,
    IUnitOfWork unitOfWork) : IRequestHandler<SendTelehealthChatMessageCommand, Guid>
{
    public async Task<Guid> Handle(SendTelehealthChatMessageCommand request, CancellationToken cancellationToken)
    {
        var patientId = currentUser.CurrentPatientId
            ?? throw new ForbiddenAccessException("A patient profile is required.");

        var userId = currentUser.CurrentUserId
            ?? throw new ForbiddenAccessException("Sign in required.");

        var session = await teleSessions.GetByIdAsync(request.TeleSessionId, cancellationToken).ConfigureAwait(false);
        if (session is null || session.PatientId != patientId)
            throw new NotFoundException(nameof(TeleSession), request.TeleSessionId);

        var entry = new TeleSessionChat
        {
            TeleSessionId = request.TeleSessionId,
            SenderUserId = userId,
            Message = request.Message.Trim(),
            SentAt = clock.UtcNow,
            IsSystemMessage = false
        };

        await chat.AddAsync(entry, cancellationToken).ConfigureAwait(false);
        await unitOfWork.SaveChangesAsync(cancellationToken).ConfigureAwait(false);
        return entry.Id;
    }
}
