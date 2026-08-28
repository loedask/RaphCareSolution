using MediatR;
using RaphCare.Application.Common.Exceptions;
using RaphCare.Application.Common.Interfaces;
using RaphCare.Application.Features.Organization.Commands.CancelAdminClinicPrescription;
using RaphCare.Application.Features.Organization.DTOs;
using RaphCare.Domain.Clinical;

namespace RaphCare.Application.Features.Organization.Commands.UndoAdminClinicPrescription;

public sealed class UndoAdminClinicPrescriptionHandler(
    ICurrentUserService currentUserService,
    IClinicStaffMembershipService clinicStaffMembershipService,
    IRepository<Visit> visitRepository,
    IRepository<Prescription> prescriptionRepository,
    IUnitOfWork unitOfWork)
    : IRequestHandler<UndoAdminClinicPrescriptionCommand, AdminClinicVisitPrescriptionDto?>
{
    public async Task<AdminClinicVisitPrescriptionDto?> Handle(
        UndoAdminClinicPrescriptionCommand request,
        CancellationToken cancellationToken)
    {
        await AdminClinicAuthorization.EnsureClinicStaffAsync(
                currentUserService,
                clinicStaffMembershipService,
                request.ClinicId,
                "Only hospital staff can undo a prescription collection.",
                cancellationToken)
            .ConfigureAwait(false);

        var prescription = await prescriptionRepository.GetByIdAsync(request.PrescriptionId, cancellationToken).ConfigureAwait(false);
        if (prescription is null)
            return null;

        var visit = await visitRepository.GetByIdAsync(prescription.VisitId, cancellationToken).ConfigureAwait(false);
        if (visit is null || visit.ClinicId != request.ClinicId)
            return null;

        if (prescription.Status is not ("Dispensed" or "Cancelled"))
            throw new BusinessRuleException("Only a collected or cancelled prescription can be put back to waiting.");

        prescription.Status = "Pending";
        prescription.DispensedAt = null;
        await prescriptionRepository.UpdateAsync(prescription, cancellationToken).ConfigureAwait(false);
        await unitOfWork.SaveChangesAsync(cancellationToken).ConfigureAwait(false);

        return await CancelAdminClinicPrescriptionHandler.MapAsync(
                prescriptionRepository, prescription, visit, cancellationToken)
            .ConfigureAwait(false);
    }
}
