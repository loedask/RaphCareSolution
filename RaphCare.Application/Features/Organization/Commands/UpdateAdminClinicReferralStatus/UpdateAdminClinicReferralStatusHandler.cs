using MediatR;
using RaphCare.Application.Common.Exceptions;
using RaphCare.Application.Common.Interfaces;
using RaphCare.Application.Features.Organization.DTOs;
using RaphCare.Domain.Clinical;
using RaphCare.Domain.Patients;

namespace RaphCare.Application.Features.Organization.Commands.UpdateAdminClinicReferralStatus;

public sealed class UpdateAdminClinicReferralStatusHandler(
    ICurrentUserService currentUserService,
    IClinicStaffMembershipService clinicStaffMembershipService,
    IRepository<Referral> referralRepository,
    IRepository<Patient> patientRepository,
    IDateTimeProvider clock,
    IUnitOfWork unitOfWork)
    : IRequestHandler<UpdateAdminClinicReferralStatusCommand, AdminClinicReferralDto?>
{
    public async Task<AdminClinicReferralDto?> Handle(
        UpdateAdminClinicReferralStatusCommand request,
        CancellationToken cancellationToken)
    {
        await AdminClinicAuthorization.EnsureClinicStaffAsync(
                currentUserService,
                clinicStaffMembershipService,
                request.ClinicId,
                "Only hospital staff can update a referral.",
                cancellationToken)
            .ConfigureAwait(false);

        var referral = await referralRepository.GetByIdAsync(request.ReferralId, cancellationToken)
            .ConfigureAwait(false);
        if (referral is null || referral.ClinicId != request.ClinicId)
            return null;

        var status = Normalize(request.Status);
        if (referral.Status is "Completed" or "Cancelled")
            throw new BusinessRuleException("This referral is already closed.");

        if (status == "Accepted" && referral.Status != "Sent")
            throw new BusinessRuleException("Only a sent referral can be marked accepted.");

        if (status == "Completed" && referral.Status is not ("Sent" or "Accepted"))
            throw new BusinessRuleException("Only an open referral can be completed.");

        if (status == "Cancelled" && referral.Status is not ("Sent" or "Accepted"))
            throw new BusinessRuleException("Only an open referral can be cancelled.");

        if (status == "Sent")
            throw new BusinessRuleException("Cannot move a referral back to Sent.");

        referral.Status = status;
        if (status == "Accepted")
            referral.AcceptedAt = clock.UtcNow;
        if (status is "Completed" or "Cancelled")
            referral.CompletedAt = clock.UtcNow;

        await referralRepository.UpdateAsync(referral, cancellationToken).ConfigureAwait(false);
        await unitOfWork.SaveChangesAsync(cancellationToken).ConfigureAwait(false);

        var patient = await patientRepository.GetByIdAsync(referral.PatientId, cancellationToken)
            .ConfigureAwait(false);
        return new AdminClinicReferralDto
        {
            Id = referral.Id,
            PatientId = referral.PatientId,
            PatientName = patient is null
                ? "Patient"
                : $"{patient.FirstName} {patient.LastName}".Trim(),
            VisitId = referral.VisitId,
            ReferredTo = referral.ReferredTo,
            Reason = referral.Reason,
            Specialty = referral.Specialty,
            Notes = referral.Notes,
            Status = referral.Status,
            ReferredAt = referral.ReferredAt,
            AcceptedAt = referral.AcceptedAt,
            CompletedAt = referral.CompletedAt
        };
    }

    private static string Normalize(string status) => status.Trim() switch
    {
        var value when value.Equals("Accepted", StringComparison.OrdinalIgnoreCase) => "Accepted",
        var value when value.Equals("Completed", StringComparison.OrdinalIgnoreCase) => "Completed",
        var value when value.Equals("Cancelled", StringComparison.OrdinalIgnoreCase) => "Cancelled",
        var value when value.Equals("Sent", StringComparison.OrdinalIgnoreCase) => "Sent",
        _ => status.Trim()
    };
}
