using MediatR;
using RaphCare.Application.Common.Interfaces;
using RaphCare.Application.Features.Organization.DTOs;
using RaphCare.Domain.Clinical;
using RaphCare.Domain.Patients;

namespace RaphCare.Application.Features.Organization.Commands.CreateAdminClinicReferral;

public sealed class CreateAdminClinicReferralHandler(
    ICurrentUserService currentUserService,
    IClinicStaffMembershipService clinicStaffMembershipService,
    IRepository<Referral> referralRepository,
    IRepository<Patient> patientRepository,
    IRepository<Visit> visitRepository,
    IDateTimeProvider clock,
    IUnitOfWork unitOfWork)
    : IRequestHandler<CreateAdminClinicReferralCommand, AdminClinicReferralDto?>
{
    public async Task<AdminClinicReferralDto?> Handle(
        CreateAdminClinicReferralCommand request,
        CancellationToken cancellationToken)
    {
        await AdminClinicAuthorization.EnsureClinicStaffAsync(
                currentUserService,
                clinicStaffMembershipService,
                request.ClinicId,
                "Only hospital staff can log a referral.",
                cancellationToken)
            .ConfigureAwait(false);

        var patient = await patientRepository.GetByIdAsync(request.PatientId, cancellationToken)
            .ConfigureAwait(false);
        if (patient is null)
            return null;

        if (request.VisitId is Guid visitId)
        {
            var visit = await visitRepository.GetByIdAsync(visitId, cancellationToken).ConfigureAwait(false);
            if (visit is null || visit.ClinicId != request.ClinicId || visit.PatientId != request.PatientId)
                return null;
        }

        var referral = new Referral
        {
            ClinicId = request.ClinicId,
            PatientId = request.PatientId,
            VisitId = request.VisitId,
            ReferredTo = request.ReferredTo.Trim(),
            Reason = string.IsNullOrWhiteSpace(request.Reason) ? null : request.Reason.Trim(),
            Specialty = string.IsNullOrWhiteSpace(request.Specialty) ? null : request.Specialty.Trim(),
            Notes = string.IsNullOrWhiteSpace(request.Notes) ? null : request.Notes.Trim(),
            Status = "Sent",
            ReferredAt = clock.UtcNow,
            CreatedByApplicationUserId = currentUserService.CurrentUserId
        };

        await referralRepository.AddAsync(referral, cancellationToken).ConfigureAwait(false);
        await unitOfWork.SaveChangesAsync(cancellationToken).ConfigureAwait(false);

        return new AdminClinicReferralDto
        {
            Id = referral.Id,
            PatientId = referral.PatientId,
            PatientName = $"{patient.FirstName} {patient.LastName}".Trim(),
            VisitId = referral.VisitId,
            ReferredTo = referral.ReferredTo,
            Reason = referral.Reason,
            Specialty = referral.Specialty,
            Notes = referral.Notes,
            Status = referral.Status,
            ReferredAt = referral.ReferredAt
        };
    }
}
