using MediatR;
using RaphCare.Application.Common.Interfaces;
using RaphCare.Application.Features.Organization.DTOs;
using RaphCare.Domain.Clinical;

namespace RaphCare.Application.Features.Organization.Commands.CreateAdminClinicVisitPrescription;

public sealed class CreateAdminClinicVisitPrescriptionHandler(
    ICurrentUserService currentUserService,
    IClinicStaffMembershipService clinicStaffMembershipService,
    IUserRoleAssignmentService roleAssignmentService,
    IRepository<Visit> visitRepository,
    IRepository<Prescription> prescriptionRepository,
    IRepository<LabRequest> labRequestRepository,
    IDateTimeProvider clock,
    IUnitOfWork unitOfWork)
    : IRequestHandler<CreateAdminClinicVisitPrescriptionCommand, AdminClinicVisitPrescriptionDto?>
{
    public async Task<AdminClinicVisitPrescriptionDto?> Handle(
        CreateAdminClinicVisitPrescriptionCommand request,
        CancellationToken cancellationToken)
    {
        var visit = await AdminClinicVisitDocumentationHelper.GetWritableVisitAsync(
                currentUserService,
                clinicStaffMembershipService,
                roleAssignmentService,
                visitRepository,
                request.ClinicId,
                request.VisitId,
                "Only hospital administrators can add prescriptions.",
                cancellationToken)
            .ConfigureAwait(false);
        if (visit is null)
            return null;

        var prescription = new Prescription
        {
            VisitId = visit.Id,
            IssuedAt = clock.UtcNow,
            Status = "Pending",
            PickupCode = await ClinicalPickupCode.AllocateAsync(
                prescriptionRepository,
                labRequestRepository,
                cancellationToken).ConfigureAwait(false),
            Notes = string.IsNullOrWhiteSpace(request.Notes) ? null : request.Notes.Trim()
        };
        prescription.PrescriptionItems.Add(new PrescriptionItem
        {
            MedicationName = request.MedicationName.Trim(),
            Dosage = string.IsNullOrWhiteSpace(request.Dosage) ? null : request.Dosage.Trim(),
            Frequency = string.IsNullOrWhiteSpace(request.Frequency) ? null : request.Frequency.Trim(),
            DurationDays = request.DurationDays
        });

        await prescriptionRepository.AddAsync(prescription, cancellationToken).ConfigureAwait(false);
        await unitOfWork.SaveChangesAsync(cancellationToken).ConfigureAwait(false);

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
            Items = prescription.PrescriptionItems.Select(i => new AdminClinicVisitPrescriptionItemDto
            {
                MedicationName = i.MedicationName,
                Dosage = i.Dosage,
                Frequency = i.Frequency,
                DurationDays = i.DurationDays
            }).ToList()
        };
    }
}
