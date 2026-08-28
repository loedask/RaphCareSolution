using MediatR;
using RaphCare.Application.Features.Organization.DTOs;

namespace RaphCare.Application.Features.Organization.Commands.CancelAdminClinicPrescription;

public sealed class CancelAdminClinicPrescriptionCommand : IRequest<AdminClinicVisitPrescriptionDto?>
{
    public Guid ClinicId { get; set; }
    public Guid PrescriptionId { get; set; }
}
