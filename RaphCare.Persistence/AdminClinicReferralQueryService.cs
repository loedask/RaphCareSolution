using Microsoft.EntityFrameworkCore;
using RaphCare.Application.Common.Interfaces;
using RaphCare.Application.Features.Organization.DTOs;
using RaphCare.Domain.Clinical;

namespace RaphCare.Persistence;

public sealed class AdminClinicReferralQueryService(ClinicalDbContext clinicalDbContext)
    : IAdminClinicReferralQueryService
{
    public async Task<AdminClinicReferralBoardDto?> GetBoardAsync(
        Guid clinicId,
        CancellationToken cancellationToken = default)
    {
        var clinic = await clinicalDbContext.Clinics
            .AsNoTracking()
            .Where(c => c.Id == clinicId && !c.IsDeleted)
            .Select(c => new { c.Name })
            .FirstOrDefaultAsync(cancellationToken)
            .ConfigureAwait(false);
        if (clinic is null)
            return null;

        var referrals = await clinicalDbContext.Referrals
            .AsNoTracking()
            .Where(r => r.ClinicId == clinicId)
            .OrderByDescending(r => r.ReferredAt)
            .Take(200)
            .ToListAsync(cancellationToken)
            .ConfigureAwait(false);

        var patientIds = referrals.Select(r => r.PatientId).Distinct().ToList();
        var patients = await clinicalDbContext.Patients
            .AsNoTracking()
            .Where(p => patientIds.Contains(p.Id))
            .Select(p => new { p.Id, p.FirstName, p.LastName })
            .ToListAsync(cancellationToken)
            .ConfigureAwait(false);
        var patientLookup = patients.ToDictionary(p => p.Id);

        AdminClinicReferralDto Map(Referral referral)
        {
            patientLookup.TryGetValue(referral.PatientId, out var patient);
            var name = patient is null
                ? "Patient"
                : $"{patient.FirstName} {patient.LastName}".Trim();
            return new AdminClinicReferralDto
            {
                Id = referral.Id,
                PatientId = referral.PatientId,
                PatientName = name,
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

        var open = referrals
            .Where(r => r.Status is "Sent" or "Accepted")
            .OrderBy(r => r.Status == "Accepted" ? 0 : 1)
            .ThenBy(r => r.ReferredAt)
            .Select(Map)
            .ToList();
        var recent = referrals
            .Where(r => r.Status is "Completed" or "Cancelled")
            .OrderByDescending(r => r.CompletedAt ?? r.UpdatedAt ?? r.ReferredAt)
            .Take(20)
            .Select(Map)
            .ToList();

        return new AdminClinicReferralBoardDto
        {
            ClinicName = clinic.Name,
            SentCount = open.Count(r => r.Status == "Sent"),
            AcceptedCount = open.Count(r => r.Status == "Accepted"),
            CompletedCount = recent.Count(r => r.Status == "Completed"),
            Open = open,
            Recent = recent
        };
    }
}
