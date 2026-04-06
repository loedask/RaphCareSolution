using MediatR;
using RaphCare.Application.Common.Exceptions;
using RaphCare.Application.Common.Interfaces;
using RaphCare.Application.Features.PatientNotifications.Commands.CreatePatientInAppNotification;
using RaphCare.Domain.Patients;

namespace RaphCare.Application.Features.PatientNotifications.Commands.RegisterMyPatientPushDevice;

public sealed class RegisterMyPatientPushDeviceHandler(
    IRepository<PatientPushDevice> devices,
    IUnitOfWork unitOfWork,
    ICurrentUserService currentUser,
    IMediator mediator) : IRequestHandler<RegisterMyPatientPushDeviceCommand, Unit>
{
    private readonly IRepository<PatientPushDevice> _devices = devices;
    private readonly IUnitOfWork _unitOfWork = unitOfWork;
    private readonly ICurrentUserService _currentUser = currentUser;
    private readonly IMediator _mediator = mediator;

    public async Task<Unit> Handle(RegisterMyPatientPushDeviceCommand request, CancellationToken cancellationToken)
    {
        var patientId = _currentUser.CurrentPatientId
            ?? throw new ForbiddenAccessException("A patient profile is required.");

        var token = request.DeviceToken.Trim();
        var platform = request.Platform.Trim().ToLowerInvariant();

        var existingForToken = await _devices.SearchAsync(
            q => q.Where(d => d.PatientId == patientId && d.DeviceToken == token),
            1,
            1,
            applyDefaultIdOrdering: false,
            cancellationToken).ConfigureAwait(false);

        if (existingForToken.TotalCount > 0)
        {
            var row = existingForToken.Items[0];
            var tracked = await _devices.GetByIdAsync(row.Id, cancellationToken).ConfigureAwait(false);
            if (tracked is not null)
            {
                tracked.Platform = platform;
                tracked.TouchRegistration();
                await _devices.UpdateAsync(tracked, cancellationToken).ConfigureAwait(false);
                await _unitOfWork.SaveChangesAsync(cancellationToken).ConfigureAwait(false);
            }

            return Unit.Value;
        }

        var countBefore = await _devices.SearchAsync(
            q => q.Where(d => d.PatientId == patientId),
            1,
            1,
            applyDefaultIdOrdering: false,
            cancellationToken).ConfigureAwait(false);
        var hadNoDevices = countBefore.TotalCount == 0;

        var device = new PatientPushDevice
        {
            PatientId = patientId,
            DeviceToken = token,
            Platform = platform,
        };
        await _devices.AddAsync(device, cancellationToken).ConfigureAwait(false);
        await _unitOfWork.SaveChangesAsync(cancellationToken).ConfigureAwait(false);

        if (hadNoDevices)
        {
            await _mediator.Send(
                new CreatePatientInAppNotificationCommand
                {
                    PatientId = patientId,
                    Title = "Push notifications",
                    Body = "This device can receive alerts from RaphCare when your clinic sends them.",
                    Type = "system",
                    SendPush = false,
                },
                cancellationToken).ConfigureAwait(false);
        }

        return Unit.Value;
    }
}
