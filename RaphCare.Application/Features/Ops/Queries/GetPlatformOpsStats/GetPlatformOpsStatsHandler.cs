using MediatR;
using RaphCare.Application.Common.Interfaces;
using RaphCare.Application.Features.Organization;
using RaphCare.Domain.Clinical;
using RaphCare.Domain.Devices;
using RaphCare.Domain.Organization;
using RaphCare.Domain.Patients;

namespace RaphCare.Application.Features.Ops.Queries.GetPlatformOpsStats;

public sealed class GetPlatformOpsStatsHandler(
    ICurrentUserService currentUserService,
    IUserRoleAssignmentService roleAssignmentService,
    IRepository<Clinic> clinicRepository,
    IRepository<Patient> patientRepository,
    IRepository<Provider> providerRepository,
    IRepository<ClinicStaffMembership> staffMembershipRepository,
    IRepository<Facility> facilityRepository,
    IRepository<Device> deviceRepository,
    IRepository<Appointment> appointmentRepository,
    IRepository<InpatientAdmission> admissionRepository,
    IRepository<ClinicStaffInvitation> invitationRepository,
    IRepository<DeviceEmergencyEvent> emergencyRepository)
    : IRequestHandler<GetPlatformOpsStatsQuery, PlatformOpsStatsDto?>
{
    public async Task<PlatformOpsStatsDto?> Handle(
        GetPlatformOpsStatsQuery request,
        CancellationToken cancellationToken)
    {
        var isPlatformAdmin = await AdminClinicAuthorization
            .IsPlatformAdministratorAsync(currentUserService, roleAssignmentService, cancellationToken)
            .ConfigureAwait(false);
        if (!isPlatformAdmin)
            return null;

        var todayStart = DateTime.UtcNow.Date;
        var tomorrowStart = todayStart.AddDays(1);
        var emergencyFrom = DateTime.UtcNow.AddHours(-72);

        // Counts must run one after another. Several repositories share a scoped DbContext,
        // and EF Core rejects overlapping operations on the same instance.
        return new PlatformOpsStatsDto
        {
            HospitalCount = await CountAsync(clinicRepository, q => q.Where(c => c.IsActive), cancellationToken)
                .ConfigureAwait(false),
            PatientCount = await CountAsync(patientRepository, q => q.Where(p => p.IsActive), cancellationToken)
                .ConfigureAwait(false),
            DoctorCount = await CountAsync(
                    providerRepository,
                    q => q.Where(p => p.IsActive && !p.IsDeleted),
                    cancellationToken)
                .ConfigureAwait(false),
            StaffCount = await CountAsync(
                    staffMembershipRepository,
                    q => q.Where(m => m.IsActive),
                    cancellationToken)
                .ConfigureAwait(false),
            FacilityCount = await CountAsync(facilityRepository, q => q, cancellationToken)
                .ConfigureAwait(false),
            DeviceCount = await CountAsync(deviceRepository, q => q, cancellationToken)
                .ConfigureAwait(false),
            UnassignedDeviceCount = await CountAsync(
                    deviceRepository,
                    q => q.Where(d => !d.IsAssigned),
                    cancellationToken)
                .ConfigureAwait(false),
            AssignedDeviceCount = await CountAsync(
                    deviceRepository,
                    q => q.Where(d => d.IsAssigned),
                    cancellationToken)
                .ConfigureAwait(false),
            AppointmentsTodayCount = await CountAsync(
                    appointmentRepository,
                    q => q.Where(a => a.ScheduledStart >= todayStart && a.ScheduledStart < tomorrowStart),
                    cancellationToken)
                .ConfigureAwait(false),
            ActiveAdmissionsCount = await CountAsync(
                    admissionRepository,
                    q => q.Where(a => a.DischargedAt == null),
                    cancellationToken)
                .ConfigureAwait(false),
            PendingStaffInvitationCount = await CountAsync(
                    invitationRepository,
                    q => q.Where(i => i.AcceptedAt == null && !i.IsCancelled),
                    cancellationToken)
                .ConfigureAwait(false),
            EmergencyEventsLast72HoursCount = await CountAsync(
                    emergencyRepository,
                    q => q.Where(e => e.OccurredAtUtc >= emergencyFrom),
                    cancellationToken)
                .ConfigureAwait(false)
        };
    }

    private static async Task<int> CountAsync<T>(
        IRepository<T> repository,
        Func<IQueryable<T>, IQueryable<T>> shaper,
        CancellationToken cancellationToken)
        where T : class
    {
        var page = await repository.SearchAsync(
            shaper,
            pageNumber: 1,
            pageSize: 1,
            applyDefaultIdOrdering: false,
            cancellationToken: cancellationToken).ConfigureAwait(false);
        return page.TotalCount;
    }
}
