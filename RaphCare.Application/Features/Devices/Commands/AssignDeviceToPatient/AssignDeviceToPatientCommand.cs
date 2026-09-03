using MediatR;
using RaphCare.Application.Common.Interfaces;

namespace RaphCare.Application.Features.Devices.Commands.AssignDeviceToPatient;

/// <summary>Assigns an in-stock fleet device to a patient (platform ops).</summary>
public sealed class AssignDeviceToPatientCommand : IRequest<Guid>, IPlatformAdminRequest
{
    public Guid DeviceId { get; set; }
    public Guid PatientId { get; set; }
}
