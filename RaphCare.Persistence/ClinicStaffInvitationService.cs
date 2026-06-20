using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;
using RaphCare.Application.Common.Configuration;
using RaphCare.Application.Common.Interfaces;
using RaphCare.Domain.Organization;

namespace RaphCare.Persistence;

public sealed class ClinicStaffInvitationService(
    ClinicalDbContext clinicalDbContext,
    IProfessionalUserLookupService professionalUserLookupService,
    IEmailService emailService,
    IClinicStaffMembershipService clinicStaffMembershipService,
    IOptions<RaphCarePortalOptions> portalOptions) : IClinicStaffInvitationService
{
    public async Task SendInvitationAsync(Guid clinicId, Guid userId, CancellationToken cancellationToken = default)
    {
        var clinic = await clinicalDbContext.Clinics
            .AsNoTracking()
            .FirstOrDefaultAsync(c => c.Id == clinicId && !c.IsDeleted, cancellationToken)
            .ConfigureAwait(false);
        if (clinic is null)
            throw new InvalidOperationException("Hospital not found.");

        var users = await professionalUserLookupService
            .GetUsersByIdsAsync([userId], cancellationToken)
            .ConfigureAwait(false);
        var user = users.FirstOrDefault();
        if (user is null)
            throw new InvalidOperationException("Staff member not found.");

        if (!await clinicStaffMembershipService
                .HasMembershipAsync(userId, clinicId, cancellationToken)
                .ConfigureAwait(false))
            throw new InvalidOperationException("Staff member is not linked to this hospital.");

        var baseUrl = portalOptions.Value.WebPortalBaseUrl.TrimEnd('/');
        var signInUrl = $"{baseUrl}/professional/signin";
        var subject = $"You're invited to {clinic.Name} on RaphCare";
        var body =
            $"""
            Hello {user.DisplayName},

            You have been invited to join {clinic.Name} on the RaphCare clinic portal.

            Sign in with your professional account to access the hospital:
            {signInUrl}

            If you have not registered yet, create a healthcare professional account first, then sign in.

            — RaphCare
            """;

        await emailService.SendEmailAsync(user.Email, subject, body, cancellationToken).ConfigureAwait(false);
        await clinicStaffMembershipService
            .RecordInvitationSentAsync(userId, clinicId, cancellationToken)
            .ConfigureAwait(false);
    }
}
