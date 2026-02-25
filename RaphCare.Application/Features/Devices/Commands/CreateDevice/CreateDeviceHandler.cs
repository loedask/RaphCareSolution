using MediatR;
using RaphCare.Application.Common.Interfaces;
using RaphCare.Domain.Devices;

namespace RaphCare.Application.Features.Devices.Commands.CreateDevice;

public class CreateDeviceHandler : IRequestHandler<CreateDeviceCommand, Guid>
{
    private readonly IRepository<Device> _repository;
    private readonly IUnitOfWork _unitOfWork;
    private readonly ICurrentUserService _currentUserService;
    private readonly IDateTimeProvider _dateTimeProvider;

    public CreateDeviceHandler(
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

    public async Task<Guid> Handle(CreateDeviceCommand request, CancellationToken cancellationToken)
    {
        var device = new Device
        {
            ClinicId = request.ClinicId,
            SerialNumber = request.SerialNumber,
            Model = request.Model,
            DeviceTypeId = request.DeviceTypeId,
            DeviceManufacturerId = request.DeviceManufacturerId,
            IsActive = true,
            Status = "Active"
        };

        await _repository.AddAsync(device, cancellationToken);
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        return device.Id;
    }
}

