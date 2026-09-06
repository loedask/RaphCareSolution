using MediatR;

namespace RaphCare.Application.Features.Organization.Commands.AssignAdminClinicDeviceToPatient;

/// <summary>Hospital staff assigns an in-stock clinic device to a patient of that hospital.</summary>
public sealed class AssignAdminClinicDeviceToPatientCommand : IRequest<Guid?>
{
    public Guid ClinicId { get; set; }
    public Guid DeviceId { get; set; }
    public Guid PatientId { get; set; }
}
