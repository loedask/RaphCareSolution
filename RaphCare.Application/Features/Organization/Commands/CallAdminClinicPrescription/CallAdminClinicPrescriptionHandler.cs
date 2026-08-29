using MediatR;
using RaphCare.Application.Common.Exceptions;
using RaphCare.Application.Common.Interfaces;
using RaphCare.Application.Features.Organization.Commands.CancelAdminClinicPrescription;
using RaphCare.Application.Features.Organization.DTOs;
using RaphCare.Domain.Clinical;

namespace RaphCare.Application.Features.Organization.Commands.CallAdminClinicPrescription;

public sealed class CallAdminClinicPrescriptionHandler(
    ICurrentUserService currentUserService,
    IClinicStaffMembershipService clinicStaffMembershipService,
    IRepository<Visit> visitRepository,
    IRepository<Prescription> prescriptionRepository,
    IDateTimeProvider clock,
    IUnitOfWork unitOfWork)
    : IRequestHandler<CallAdminClinicPrescriptionCommand, AdminClinicVisitPrescriptionDto?>
{
    public async Task<AdminClinicVisitPrescriptionDto?> Handle(
        CallAdminClinicPrescriptionCommand request,
        CancellationToken cancellationToken)
    {
        await AdminClinicAuthorization.EnsureClinicStaffAsync(
                currentUserService,
                clinicStaffMembershipService,
                request.ClinicId,
                "Only hospital staff can call a pickup code.",
                cancellationToken)
            .ConfigureAwait(false);

        var prescription = await prescriptionRepository.GetByIdAsync(request.PrescriptionId, cancellationToken)
            .ConfigureAwait(false);
        if (prescription is null)
            return null;

        var visit = await visitRepository.GetByIdAsync(prescription.VisitId, cancellationToken).ConfigureAwait(false);
        if (visit is null || visit.ClinicId != request.ClinicId)
            return null;

        if (prescription.Status != "Pending")
            throw new BusinessRuleException("Only a waiting prescription can be called.");

        prescription.CalledAt = clock.UtcNow;
        await prescriptionRepository.UpdateAsync(prescription, cancellationToken).ConfigureAwait(false);
        await unitOfWork.SaveChangesAsync(cancellationToken).ConfigureAwait(false);

        return await CancelAdminClinicPrescriptionHandler.MapAsync(
                prescriptionRepository, prescription, visit, cancellationToken)
            .ConfigureAwait(false);
    }
}
