using RaphCare.Application.Common.Exceptions;
using RaphCare.Application.Common.Interfaces;
using RaphCare.Domain.Clinical;
using RaphCare.Domain.Common;
using RaphCare.Domain.Organization;

namespace RaphCare.Application.Features.Organization;

/// <summary>Shared conflict and weekly-schedule checks for admin clinic booking.</summary>
internal static class AppointmentSchedulingGuard
{
    public static async Task EnsureNoProviderConflictAsync(
        IRepository<Appointment> appointmentRepository,
        Guid clinicId,
        Guid providerId,
        DateTime scheduledStart,
        DateTime scheduledEnd,
        Guid? excludeAppointmentId,
        CancellationToken cancellationToken)
    {
        var conflictPage = await appointmentRepository.SearchAsync(
            q => q.Where(a =>
                a.ClinicId == clinicId
                && a.ProviderId == providerId
                && !a.IsCancelled
                && (excludeAppointmentId == null || a.Id != excludeAppointmentId.Value)
                && a.ScheduledStart < scheduledEnd
                && a.ScheduledEnd > scheduledStart),
            1,
            1,
            applyDefaultIdOrdering: false,
            cancellationToken).ConfigureAwait(false);

        if (conflictPage.TotalCount > 0)
            throw new BusinessRuleException("This provider already has an appointment overlapping that time.");
    }

    public static void EnsureFitsWeeklySchedule(
        IReadOnlyList<ProviderSchedule> schedules,
        DateTime scheduledStartUtc,
        DateTime scheduledEndUtc,
        string? clinicTimeZone)
    {
        if (schedules.Count == 0)
            return;

        var localStart = ClinicTimeZoneHelper.ToClinicLocal(scheduledStartUtc, clinicTimeZone);
        var localEnd = ClinicTimeZoneHelper.ToClinicLocal(scheduledEndUtc, clinicTimeZone);

        if (localStart.Date != localEnd.Date)
            throw new BusinessRuleException("Appointments must start and end on the same hospital calendar day.");

        var day = localStart.DayOfWeek;
        var startTod = localStart.TimeOfDay;
        var endTod = localEnd.TimeOfDay;

        var fits = schedules.Any(s =>
            s.Day == day
            && startTod >= s.StartTime
            && endTod <= s.EndTime);

        if (!fits)
            throw new BusinessRuleException(
                "Appointment time is outside the provider's weekly schedule for that day.");
    }
}
