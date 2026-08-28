using MediatR;

namespace RaphCare.Application.Features.Organization.Commands.RevokeAdminClinicPatientAccess;

public sealed class RevokeAdminClinicPatientAccessCommand : IRequest<bool>
{
    public Guid ClinicId { get; set; }
    public Guid PatientId { get; set; }
}
