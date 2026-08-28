using MediatR;
using RaphCare.Application.Features.Organization.DTOs;

namespace RaphCare.Application.Features.Organization.Commands.UndoAdminClinicPrescription;

public sealed class UndoAdminClinicPrescriptionCommand : IRequest<AdminClinicVisitPrescriptionDto?>
{
    public Guid ClinicId { get; set; }
    public Guid PrescriptionId { get; set; }
}
