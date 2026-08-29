using MediatR;
using RaphCare.Application.Features.Organization.DTOs;

namespace RaphCare.Application.Features.Organization.Commands.CallAdminClinicPrescription;

public sealed class CallAdminClinicPrescriptionCommand : IRequest<AdminClinicVisitPrescriptionDto?>
{
    public Guid ClinicId { get; set; }
    public Guid PrescriptionId { get; set; }
}
