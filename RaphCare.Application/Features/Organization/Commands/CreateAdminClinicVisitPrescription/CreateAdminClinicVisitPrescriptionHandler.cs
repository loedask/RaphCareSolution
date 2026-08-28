using MediatR;
using RaphCare.Application.Common.Interfaces;
using RaphCare.Application.Features.Organization.DTOs;
using RaphCare.Domain.Clinical;
using RaphCare.Domain.Organization;

namespace RaphCare.Application.Features.Organization.Commands.CreateAdminClinicVisitPrescription;

public sealed class CreateAdminClinicVisitPrescriptionHandler(
    ICurrentUserService currentUserService,
    IClinicStaffMembershipService clinicStaffMembershipService,
    IUserRoleAssignmentService roleAssignmentService,
    IRepository<Visit> visitRepository,
    IRepository<Prescription> prescriptionRepository,
    IRepository<LabRequest> labRequestRepository,
    IRepository<Clinic> clinicRepository,
    IDateTimeProvider clock,
    IUnitOfWork unitOfWork,
    IMediator mediator)
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

        var lines = request.Items
            .Where(i => !string.IsNullOrWhiteSpace(i.MedicationName))
            .ToList();
        if (lines.Count == 0 && !string.IsNullOrWhiteSpace(request.MedicationName))
        {
            lines.Add(new CreateAdminClinicVisitPrescriptionLine
            {
                MedicationName = request.MedicationName,
                Dosage = request.Dosage,
                Frequency = request.Frequency,
                DurationDays = request.DurationDays
            });
        }

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

        foreach (var line in lines)
        {
            prescription.PrescriptionItems.Add(new PrescriptionItem
            {
                MedicationName = line.MedicationName.Trim(),
                Dosage = string.IsNullOrWhiteSpace(line.Dosage) ? null : line.Dosage.Trim(),
                Frequency = string.IsNullOrWhiteSpace(line.Frequency) ? null : line.Frequency.Trim(),
                DurationDays = line.DurationDays
            });
        }

        await prescriptionRepository.AddAsync(prescription, cancellationToken).ConfigureAwait(false);
        await unitOfWork.SaveChangesAsync(cancellationToken).ConfigureAwait(false);

        var clinic = await clinicRepository.GetByIdAsync(visit.ClinicId, cancellationToken).ConfigureAwait(false);
        await CollectionPatientNotifier.NotifyReadyAsync(
                mediator,
                visit.PatientId,
                clinic?.Name ?? string.Empty,
                prescription.PickupCode,
                isLab: false,
                cancellationToken)
            .ConfigureAwait(false);

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
