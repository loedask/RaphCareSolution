using Microsoft.EntityFrameworkCore;
using RaphCare.Application.Common.Interfaces;
using RaphCare.Application.Features.Organization.DTOs;
using RaphCare.Domain.Common;

namespace RaphCare.Persistence;

public sealed class AdminClinicDashboardQueryService(
    ClinicalDbContext clinicalDbContext,
    IClinicStaffMembershipService clinicStaffMembershipService,
    IAdminClinicAppointmentQueryService adminClinicAppointmentQueryService,
    IDateTimeProvider clock)
    : IAdminClinicDashboardQueryService
{
    public async Task<AdminClinicDashboardDto?> GetDashboardAsync(
        Guid clinicId,
        CancellationToken cancellationToken = default)
    {
        var clinic = await clinicalDbContext.Clinics
            .AsNoTracking()
            .Include(c => c.Facilities)
            .FirstOrDefaultAsync(c => c.Id == clinicId && !c.IsDeleted, cancellationToken)
            .ConfigureAwait(false);

        if (clinic is null)
            return null;

        var patientCount = await clinicalDbContext.PatientClinicAccesses
            .AsNoTracking()
            .CountAsync(a => a.ClinicId == clinicId && a.IsActive, cancellationToken)
            .ConfigureAwait(false);

        var staffCount = await clinicStaffMembershipService
            .GetActiveStaffCountAsync(clinicId, cancellationToken)
            .ConfigureAwait(false);

        var providerCount = await clinicalDbContext.Set<Domain.Organization.Provider>()
            .AsNoTracking()
            .CountAsync(p => p.ClinicId == clinicId && !p.IsDeleted && p.IsActive, cancellationToken)
            .ConfigureAwait(false);

        var todayUtc = clock.UtcNow;
        var (todayStartUtc, todayEndUtc) = ClinicTimeZoneHelper.GetClinicDayUtcRange(todayUtc, clinic.TimeZone);
        var weekAheadUtc = todayStartUtc.AddDays(7);

        var appointmentsTodayCount = await clinicalDbContext.Appointments
            .AsNoTracking()
            .CountAsync(
                a => a.ClinicId == clinicId && !a.IsCancelled
                     && a.ScheduledStart >= todayStartUtc && a.ScheduledStart <= todayEndUtc,
                cancellationToken)
            .ConfigureAwait(false);

        var upcomingAppointments = await adminClinicAppointmentQueryService
            .GetAppointmentsAsync(clinicId, 1, 5, todayStartUtc, weekAheadUtc, cancellationToken: cancellationToken)
            .ConfigureAwait(false);

        var upcomingCount = await clinicalDbContext.Appointments
            .AsNoTracking()
            .CountAsync(
                a => a.ClinicId == clinicId && !a.IsCancelled
                     && a.ScheduledStart >= todayStartUtc && a.ScheduledStart < weekAheadUtc,
                cancellationToken)
            .ConfigureAwait(false);

        return new AdminClinicDashboardDto
        {
            ClinicId = clinic.Id,
            ClinicName = clinic.Name,
            PatientCount = patientCount,
            StaffCount = staffCount,
            ProviderCount = providerCount,
            FacilityCount = clinic.Facilities.Count,
            AppointmentsTodayCount = appointmentsTodayCount,
            UpcomingAppointmentsCount = upcomingCount,
            UpcomingAppointments = upcomingAppointments.Items
        };
    }
}
