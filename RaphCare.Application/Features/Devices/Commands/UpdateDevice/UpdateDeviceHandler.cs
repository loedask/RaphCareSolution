using MediatR;
using RaphCare.Application.Common.Exceptions;
using RaphCare.Application.Common.Interfaces;
using RaphCare.Domain.Devices;

namespace RaphCare.Application.Features.Devices.Commands.UpdateDevice;

public class UpdateDeviceHandler : IRequestHandler<UpdateDeviceCommand, Unit>
{
    private readonly IRepository<Device> _repository;
    private readonly IUnitOfWork _unitOfWork;
    private readonly ICurrentUserService _currentUserService;
    private readonly IDateTimeProvider _dateTimeProvider;

    public UpdateDeviceHandler(
        IRepository<Device> repository,
        IUnitOfWork unitOfWork,
        ICurrentUserService currentUserService,
        IDateTimeProvider dateTimeProvider)
    {
        _repository = repository;
        _unitOfWork = unitOfWork;
        _currentUserService = currentUserService;
        _dateTimeProvider = dateTimeProvider;
    }

    public async Task<Unit> Handle(UpdateDeviceCommand request, CancellationToken cancellationToken)
    {
        var device = await _repository.GetByIdAsync(request.Id, cancellationToken);
        if (device is null)
        {
            throw new NotFoundException(nameof(Device), request.Id);
        }

        if (request.IsActive.HasValue)
        {
            device.IsActive = request.IsActive.Value;
        }

        if (request.IsAssigned.HasValue)
        {
            device.IsAssigned = request.IsAssigned.Value;
        }

        if (!string.IsNullOrWhiteSpace(request.Status))
        {
            device.Status = request.Status;
        }

        await _repository.UpdateAsync(device, cancellationToken);
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        return Unit.Value;
    }
}

