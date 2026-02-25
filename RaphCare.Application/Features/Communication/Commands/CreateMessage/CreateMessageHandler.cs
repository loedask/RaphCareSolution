using MediatR;
using RaphCare.Application.Common.Interfaces;
using RaphCare.Domain.Communication;

namespace RaphCare.Application.Features.Communication.Commands.CreateMessage;

public class CreateMessageHandler : IRequestHandler<CreateMessageCommand, Guid>
{
    private readonly IRepository<Message> _repository;
    private readonly IUnitOfWork _unitOfWork;
    private readonly ICurrentUserService _currentUserService;
    private readonly IDateTimeProvider _dateTimeProvider;

    public CreateMessageHandler(
        IRepository<Message> repository,
        IUnitOfWork unitOfWork,
        ICurrentUserService currentUserService,
        IDateTimeProvider dateTimeProvider)
    {
        _repository = repository;
        _unitOfWork = unitOfWork;
        _currentUserService = currentUserService;
        _dateTimeProvider = dateTimeProvider;
    }

    public async Task<Guid> Handle(CreateMessageCommand request, CancellationToken cancellationToken)
    {
        var message = new Message
        {
            ClinicId = request.ClinicId,
            RecipientUserId = request.RecipientUserId,
            Channel = request.Channel,
            Subject = request.Subject,
            Body = request.Body,
            IsSent = false
        };

        await _repository.AddAsync(message, cancellationToken);
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        return message.Id;
    }
}
