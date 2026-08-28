using MediatR;
using Microsoft.EntityFrameworkCore;
using RaphCare.Application.Common.Exceptions;
using RaphCare.Application.Common.Interfaces;
using RaphCare.Application.Features.Organization.DTOs;
using RaphCare.Domain.Clinical;

namespace RaphCare.Application.Features.Organization.Commands.DispenseAdminClinicPrescription;

public sealed class DispenseAdminClinicPrescriptionHandler(
    ICurrentUserService currentUserService,
    IClinicStaffMembershipService clinicStaffMembershipService,
    IRepository<Visit> visitRepository,
    IRepository<Prescription> prescriptionRepository,
    IDateTimeProvider clock,
    IUnitOfWork unitOfWork)
    : IRequestHandler<DispenseAdminClinicPrescriptionCommand, AdminClinicVisitPrescriptionDto?>
{
    public async Task<AdminClinicVisitPrescriptionDto?> Handle(
        DispenseAdminClinicPrescriptionCommand request,
        CancellationToken cancellationToken)
    {
        await AdminClinicAuthorization.EnsureClinicStaffAsync(
                currentUserService,
                clinicStaffMembershipService,
                request.ClinicId,
                "Only hospital staff can mark a prescription as collected.",
                cancellationToken)
            .ConfigureAwait(false);

        var prescription = await prescriptionRepository.GetByIdAsync(request.PrescriptionId, cancellationToken).ConfigureAwait(false);
        if (prescription is null)
            return null;

        var visit = await visitRepository.GetByIdAsync(prescription.VisitId, cancellationToken).ConfigureAwait(false);
        if (visit is null || visit.ClinicId != request.ClinicId)
            return null;

        if (prescription.Status == "Dispensed")
            throw new BusinessRuleException("This prescription was already collected.");

        prescription.Status = "Dispensed";
        prescription.DispensedAt = clock.UtcNow;
        await prescriptionRepository.UpdateAsync(prescription, cancellationToken).ConfigureAwait(false);
        await unitOfWork.SaveChangesAsync(cancellationToken).ConfigureAwait(false);

        var withItems = await prescriptionRepository.SearchAsync(
            q => q.Where(p => p.Id == prescription.Id).Include(p => p.PrescriptionItems),
            1,
            1,
            cancellationToken: cancellationToken).ConfigureAwait(false);
        var items = withItems.Items.Count > 0
            ? withItems.Items[0].PrescriptionItems
            : prescription.PrescriptionItems;

        return new AdminClinicVisitPrescriptionDto
        {
            Id = prescription.Id,
            VisitId = visit.Id,
            VisitStart = visit.VisitStart,
            IssuedAt = prescription.IssuedAt,
            Status = prescription.Status,
            PickupCode = prescription.PickupCode,
            DispensedAt = prescription.DispensedAt,
            Notes = prescription.Notes,
            Items = items.Select(i => new AdminClinicVisitPrescriptionItemDto
            {
                MedicationName = i.MedicationName,
                Dosage = i.Dosage,
                Frequency = i.Frequency,
                DurationDays = i.DurationDays
            }).ToList()
        };
    }
}
