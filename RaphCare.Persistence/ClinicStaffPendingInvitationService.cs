using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;
using RaphCare.Application.Common.Configuration;
using RaphCare.Application.Common.Email;
using RaphCare.Application.Common.Interfaces;
using RaphCare.Domain.Organization;

namespace RaphCare.Persistence;

public sealed class ClinicStaffPendingInvitationService(
    ClinicalDbContext clinicalDbContext,
    IClinicStaffMembershipService clinicStaffMembershipService,
    IEmailService emailService,
    IOptions<RaphCarePortalOptions> portalOptions) : IClinicStaffPendingInvitationService
{
    public async Task<PendingStaffInvitationEntry> CreateAsync(
        Guid clinicId,
        string email,
        Guid invitedByApplicationUserId,
        CancellationToken cancellationToken = default)
    {
        var normalizedEmail = NormalizeEmail(email);
        var existingPending = await clinicalDbContext.ClinicStaffInvitations
            .FirstOrDefaultAsync(
                i => i.ClinicId == clinicId
                     && i.Email == normalizedEmail
                     && !i.IsCancelled
                     && i.AcceptedAt == null,
                cancellationToken)
            .ConfigureAwait(false);

        if (existingPending is not null)
        {
            await SendRegistrationEmailAsync(existingPending, cancellationToken).ConfigureAwait(false);
            existingPending.LastInvitationSentAt = DateTime.UtcNow;
            await clinicalDbContext.SaveChangesAsync(cancellationToken).ConfigureAwait(false);
            return Map(existingPending);
        }

        var invitation = new ClinicStaffInvitation
        {
            ClinicId = clinicId,
            Email = normalizedEmail,
            InvitedByApplicationUserId = invitedByApplicationUserId,
            InvitedAt = DateTime.UtcNow,
            LastInvitationSentAt = DateTime.UtcNow
        };

        clinicalDbContext.ClinicStaffInvitations.Add(invitation);
        await clinicalDbContext.SaveChangesAsync(cancellationToken).ConfigureAwait(false);
        await SendRegistrationEmailAsync(invitation, cancellationToken).ConfigureAwait(false);
        return Map(invitation);
    }

    public async Task<IReadOnlyList<PendingStaffInvitationEntry>> GetPendingForClinicAsync(
        Guid clinicId,
        CancellationToken cancellationToken = default)
    {
        return await clinicalDbContext.ClinicStaffInvitations
            .AsNoTracking()
            .Where(i => i.ClinicId == clinicId && !i.IsCancelled && i.AcceptedAt == null)
            .OrderBy(i => i.InvitedAt)
            .Select(i => new PendingStaffInvitationEntry
            {
                InvitationId = i.Id,
                Email = i.Email,
                InvitedAt = i.InvitedAt,
                LastInvitationSentAt = i.LastInvitationSentAt
            })
            .ToListAsync(cancellationToken)
            .ConfigureAwait(false);
    }

    public async Task<bool> ResendAsync(Guid clinicId, Guid invitationId, CancellationToken cancellationToken = default)
    {
        var invitation = await clinicalDbContext.ClinicStaffInvitations
            .FirstOrDefaultAsync(
                i => i.Id == invitationId
                     && i.ClinicId == clinicId
                     && !i.IsCancelled
                     && i.AcceptedAt == null,
                cancellationToken)
            .ConfigureAwait(false);

        if (invitation is null)
            return false;

        await SendRegistrationEmailAsync(invitation, cancellationToken).ConfigureAwait(false);
        invitation.LastInvitationSentAt = DateTime.UtcNow;
        await clinicalDbContext.SaveChangesAsync(cancellationToken).ConfigureAwait(false);
        return true;
    }

    public async Task<bool> CancelAsync(Guid clinicId, Guid invitationId, CancellationToken cancellationToken = default)
    {
        var invitation = await clinicalDbContext.ClinicStaffInvitations
            .FirstOrDefaultAsync(
                i => i.Id == invitationId
                     && i.ClinicId == clinicId
                     && !i.IsCancelled
                     && i.AcceptedAt == null,
                cancellationToken)
            .ConfigureAwait(false);

        if (invitation is null)
            return false;

        invitation.IsCancelled = true;
        await clinicalDbContext.SaveChangesAsync(cancellationToken).ConfigureAwait(false);
        return true;
    }

    public async Task AcceptPendingInvitationsAsync(
        Guid applicationUserId,
        string email,
        CancellationToken cancellationToken = default)
    {
        var normalizedEmail = NormalizeEmail(email);
        var pending = await clinicalDbContext.ClinicStaffInvitations
            .Where(i => i.Email == normalizedEmail && !i.IsCancelled && i.AcceptedAt == null)
            .ToListAsync(cancellationToken)
            .ConfigureAwait(false);

        if (pending.Count == 0)
            return;

        foreach (var invitation in pending)
        {
            await clinicStaffMembershipService
                .EnsureMembershipAsync(applicationUserId, invitation.ClinicId, cancellationToken)
                .ConfigureAwait(false);

            invitation.AcceptedAt = DateTime.UtcNow;
            invitation.AcceptedApplicationUserId = applicationUserId;
        }

        await clinicalDbContext.SaveChangesAsync(cancellationToken).ConfigureAwait(false);
    }

    private async Task SendRegistrationEmailAsync(
        ClinicStaffInvitation invitation,
        CancellationToken cancellationToken)
    {
        var clinic = await clinicalDbContext.Clinics
            .AsNoTracking()
            .FirstOrDefaultAsync(c => c.Id == invitation.ClinicId, cancellationToken)
            .ConfigureAwait(false);
        if (clinic is null)
            return;

        var baseUrl = portalOptions.Value.WebPortalBaseUrl.TrimEnd('/');
        var registerUrl = $"{baseUrl}/professional/register?email={Uri.EscapeDataString(invitation.Email)}";
        var content = StaffInvitationEmail.ForNewUser(clinic.Name, registerUrl);
        await emailService
            .SendEmailAsync(invitation.Email, content.Subject, content.PlainBody, content.HtmlBody, cancellationToken)
            .ConfigureAwait(false);
    }

    private static PendingStaffInvitationEntry Map(ClinicStaffInvitation invitation) => new()
    {
        InvitationId = invitation.Id,
        Email = invitation.Email,
        InvitedAt = invitation.InvitedAt,
        LastInvitationSentAt = invitation.LastInvitationSentAt
    };

    private static string NormalizeEmail(string email) => email.Trim().ToLowerInvariant();
}
