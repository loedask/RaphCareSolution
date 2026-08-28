using MediatR;
using RaphCare.Application.Features.Organization.DTOs;

namespace RaphCare.Application.Features.Organization.Commands.DispenseAdminClinicPrescription;

public sealed class DispenseAdminClinicPrescriptionCommand : IRequest<AdminClinicVisitPrescriptionDto?>
{
    public Guid ClinicId { get; set; }
    public Guid PrescriptionId { get; set; }
}
