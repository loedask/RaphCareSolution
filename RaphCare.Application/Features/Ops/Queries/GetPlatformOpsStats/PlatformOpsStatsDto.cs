namespace RaphCare.Application.Features.Ops.Queries.GetPlatformOpsStats;

/// <summary>Platform-wide totals for the Ops home dashboard.</summary>
public sealed class PlatformOpsStatsDto
{
    public int HospitalCount { get; set; }
    public int PatientCount { get; set; }
    public int DoctorCount { get; set; }
    public int StaffCount { get; set; }
    public int FacilityCount { get; set; }
    public int DeviceCount { get; set; }
    public int UnassignedDeviceCount { get; set; }
    public int AssignedDeviceCount { get; set; }
    public int AppointmentsTodayCount { get; set; }
    public int ActiveAdmissionsCount { get; set; }
    public int PendingStaffInvitationCount { get; set; }
    public int EmergencyEventsLast72HoursCount { get; set; }
}
