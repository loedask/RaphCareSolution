using Microsoft.EntityFrameworkCore;
using RaphCare.Application.Common.Interfaces;
using RaphCare.Domain.Organization;

namespace RaphCare.Persistence;

public sealed class ClinicStaffMembershipService(ClinicalDbContext clinicalDbContext) : IClinicStaffMembershipService
{
    public async Task EnsureMembershipAsync(Guid applicationUserId, Guid clinicId, CancellationToken cancellationToken = default)
    {
        var existing = await clinicalDbContext.ClinicStaffMemberships
            .FirstOrDefaultAsync(
                m => m.ApplicationUserId == applicationUserId && m.ClinicId == clinicId,
                cancellationToken)
            .ConfigureAwait(false);

        if (existing is not null)
        {
            if (!existing.IsActive)
            {
                existing.IsActive = true;
                existing.JoinedAt = DateTime.UtcNow;
                await clinicalDbContext.SaveChangesAsync(cancellationToken).ConfigureAwait(false);
            }

            return;
        }

        clinicalDbContext.ClinicStaffMemberships.Add(new ClinicStaffMembership
        {
            ApplicationUserId = applicationUserId,
            ClinicId = clinicId,
            IsActive = true,
            JoinedAt = DateTime.UtcNow
        });

        var clinic = await clinicalDbContext.Clinics
            .FirstOrDefaultAsync(c => c.Id == clinicId, cancellationToken)
            .ConfigureAwait(false);
        if (clinic is not null && clinic.RegisteredByApplicationUserId is null)
            clinic.RegisteredByApplicationUserId = applicationUserId;

        await clinicalDbContext.SaveChangesAsync(cancellationToken).ConfigureAwait(false);
    }

    public async Task<IReadOnlyList<Guid>> GetClinicIdsForUserAsync(
        Guid applicationUserId,
        CancellationToken cancellationToken = default)
    {
        return await clinicalDbContext.ClinicStaffMemberships
            .Where(m => m.ApplicationUserId == applicationUserId && m.IsActive)
            .Select(m => m.ClinicId)
            .ToListAsync(cancellationToken)
            .ConfigureAwait(false);
    }

    public Task<bool> HasMembershipAsync(
        Guid applicationUserId,
        Guid clinicId,
        CancellationToken cancellationToken = default) =>
        clinicalDbContext.ClinicStaffMemberships
            .AnyAsync(m => m.ApplicationUserId == applicationUserId && m.ClinicId == clinicId && m.IsActive, cancellationToken);

    public Task<int> GetActiveStaffCountAsync(Guid clinicId, CancellationToken cancellationToken = default) =>
        clinicalDbContext.ClinicStaffMemberships
            .CountAsync(m => m.ClinicId == clinicId && m.IsActive, cancellationToken);

    public async Task<IReadOnlyList<ClinicStaffMembershipEntry>> GetStaffMembershipsAsync(
        Guid clinicId,
        CancellationToken cancellationToken = default)
    {
        return await clinicalDbContext.ClinicStaffMemberships
            .AsNoTracking()
            .Where(m => m.ClinicId == clinicId)
            .OrderBy(m => m.JoinedAt)
            .Select(m => new ClinicStaffMembershipEntry
            {
                ApplicationUserId = m.ApplicationUserId,
                JoinedAt = m.JoinedAt,
                IsActive = m.IsActive
            })
            .ToListAsync(cancellationToken)
            .ConfigureAwait(false);
    }

    public async Task<bool> DeactivateMembershipAsync(
        Guid applicationUserId,
        Guid clinicId,
        CancellationToken cancellationToken = default)
    {
        var membership = await clinicalDbContext.ClinicStaffMemberships
            .FirstOrDefaultAsync(
                m => m.ApplicationUserId == applicationUserId && m.ClinicId == clinicId && m.IsActive,
                cancellationToken)
            .ConfigureAwait(false);

        if (membership is null)
            return false;

        membership.IsActive = false;
        await clinicalDbContext.SaveChangesAsync(cancellationToken).ConfigureAwait(false);
        return true;
    }
}
