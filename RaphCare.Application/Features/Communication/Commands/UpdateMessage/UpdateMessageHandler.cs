using MediatR;
using RaphCare.Application.Common.Exceptions;
using RaphCare.Application.Common.Interfaces;
using RaphCare.Domain.Communication;

namespace RaphCare.Application.Features.Communication.Commands.UpdateMessage;

public class UpdateMessageHandler(
    IRepository<Message> repository,
    IUnitOfWork unitOfWork,
    ICurrentUserService currentUserService,
    IDateTimeProvider dateTimeProvider) : IRequestHandler<UpdateMessageCommand, Unit>
{
    private readonly IRepository<Message> _repository = repository;
    private readonly IUnitOfWork _unitOfWork = unitOfWork;
    private readonly ICurrentUserService _currentUserService = currentUserService;
    private readonly IDateTimeProvider _dateTimeProvider = dateTimeProvider;

    public async Task<Unit> Handle(UpdateMessageCommand request, CancellationToken cancellationToken)
    {
        var message = await _repository.GetByIdAsync(request.Id, cancellationToken);
        if (message is null)
        {
            throw new NotFoundException(nameof(Message), request.Id);
        }

        if (request.SentAt.HasValue)
        {
            message.SentAt = request.SentAt.Value;
        }

        if (request.IsSent.HasValue)
        {
            message.IsSent = request.IsSent.Value;
        }

        await _repository.UpdateAsync(message, cancellationToken);
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        return Unit.Value;
    }
}
