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

        var hospitalsTask = CountAsync(clinicRepository, q => q.Where(c => c.IsActive), cancellationToken);
        var patientsTask = CountAsync(patientRepository, q => q.Where(p => p.IsActive), cancellationToken);
        var doctorsTask = CountAsync(
            providerRepository,
            q => q.Where(p => p.IsActive && !p.IsDeleted),
            cancellationToken);
        var staffTask = CountAsync(
            staffMembershipRepository,
            q => q.Where(m => m.IsActive),
            cancellationToken);
        var facilitiesTask = CountAsync(facilityRepository, q => q, cancellationToken);
        var devicesTask = CountAsync(deviceRepository, q => q, cancellationToken);
        var unassignedTask = CountAsync(deviceRepository, q => q.Where(d => !d.IsAssigned), cancellationToken);
        var assignedTask = CountAsync(deviceRepository, q => q.Where(d => d.IsAssigned), cancellationToken);
        var appointmentsTask = CountAsync(
            appointmentRepository,
            q => q.Where(a => a.ScheduledStart >= todayStart && a.ScheduledStart < tomorrowStart),
            cancellationToken);
        var admissionsTask = CountAsync(
            admissionRepository,
            q => q.Where(a => a.DischargedAt == null),
            cancellationToken);
        var invitationsTask = CountAsync(
            invitationRepository,
            q => q.Where(i => i.AcceptedAt == null && !i.IsCancelled),
            cancellationToken);
        var emergenciesTask = CountAsync(
            emergencyRepository,
            q => q.Where(e => e.OccurredAtUtc >= emergencyFrom),
            cancellationToken);

        await Task.WhenAll(
            hospitalsTask,
            patientsTask,
            doctorsTask,
            staffTask,
            facilitiesTask,
            devicesTask,
            unassignedTask,
            assignedTask,
            appointmentsTask,
            admissionsTask,
            invitationsTask,
            emergenciesTask).ConfigureAwait(false);

        return new PlatformOpsStatsDto
        {
            HospitalCount = hospitalsTask.Result,
            PatientCount = patientsTask.Result,
            DoctorCount = doctorsTask.Result,
            StaffCount = staffTask.Result,
            FacilityCount = facilitiesTask.Result,
            DeviceCount = devicesTask.Result,
            UnassignedDeviceCount = unassignedTask.Result,
            AssignedDeviceCount = assignedTask.Result,
            AppointmentsTodayCount = appointmentsTask.Result,
            ActiveAdmissionsCount = admissionsTask.Result,
            PendingStaffInvitationCount = invitationsTask.Result,
            EmergencyEventsLast72HoursCount = emergenciesTask.Result
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
