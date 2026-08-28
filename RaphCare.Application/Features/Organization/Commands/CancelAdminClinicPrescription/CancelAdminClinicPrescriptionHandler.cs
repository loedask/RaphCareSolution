using MediatR;
using Microsoft.EntityFrameworkCore;
using RaphCare.Application.Common.Exceptions;
using RaphCare.Application.Common.Interfaces;
using RaphCare.Application.Features.Organization.DTOs;
using RaphCare.Domain.Clinical;

namespace RaphCare.Application.Features.Organization.Commands.CancelAdminClinicPrescription;

public sealed class CancelAdminClinicPrescriptionHandler(
    ICurrentUserService currentUserService,
    IClinicStaffMembershipService clinicStaffMembershipService,
    IRepository<Visit> visitRepository,
    IRepository<Prescription> prescriptionRepository,
    IUnitOfWork unitOfWork)
    : IRequestHandler<CancelAdminClinicPrescriptionCommand, AdminClinicVisitPrescriptionDto?>
{
    public async Task<AdminClinicVisitPrescriptionDto?> Handle(
        CancelAdminClinicPrescriptionCommand request,
        CancellationToken cancellationToken)
    {
        await AdminClinicAuthorization.EnsureClinicStaffAsync(
                currentUserService,
                clinicStaffMembershipService,
                request.ClinicId,
                "Only hospital staff can cancel a prescription.",
                cancellationToken)
            .ConfigureAwait(false);

        var prescription = await prescriptionRepository.GetByIdAsync(request.PrescriptionId, cancellationToken).ConfigureAwait(false);
        if (prescription is null)
            return null;

        var visit = await visitRepository.GetByIdAsync(prescription.VisitId, cancellationToken).ConfigureAwait(false);
        if (visit is null || visit.ClinicId != request.ClinicId)
            return null;

        if (prescription.Status != "Pending")
            throw new BusinessRuleException("Only a waiting prescription can be cancelled.");

        prescription.Status = "Cancelled";
        await prescriptionRepository.UpdateAsync(prescription, cancellationToken).ConfigureAwait(false);
        await unitOfWork.SaveChangesAsync(cancellationToken).ConfigureAwait(false);

        return await MapAsync(prescriptionRepository, prescription, visit, cancellationToken).ConfigureAwait(false);
    }

    internal static async Task<AdminClinicVisitPrescriptionDto> MapAsync(
        IRepository<Prescription> prescriptionRepository,
        Prescription prescription,
        Visit visit,
        CancellationToken cancellationToken)
    {
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
