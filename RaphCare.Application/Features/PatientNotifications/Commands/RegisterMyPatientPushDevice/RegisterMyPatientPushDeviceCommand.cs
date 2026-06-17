using MediatR;

namespace RaphCare.Application.Features.PatientNotifications.Commands.RegisterMyPatientPushDevice;

/// <summary>Registers or refreshes a device token for push (FCM/APNs).</summary>
public sealed class RegisterMyPatientPushDeviceCommand : IRequest<Unit>
{
    public string DeviceToken { get; set; } = string.Empty;
    public string Platform { get; set; } = string.Empty;
}
