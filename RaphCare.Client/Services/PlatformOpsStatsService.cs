using RaphCare.Client.Contracts;
using RaphCare.Client.Contracts.Interfaces;
using RaphCare.Client.Models.Ops;
using RaphCare.Client.Services.Base;

namespace RaphCare.Client.Services;

public sealed class PlatformOpsStatsService(HttpClient httpClient) : BaseHttpService(httpClient), IPlatformOpsStatsService
{
    public async Task<Response<PlatformOpsStats>> GetStatsAsync(CancellationToken cancellationToken = default)
    {
        var result = await GetAsync<PlatformOpsStatsDto>("api/ops/stats", cancellationToken)
            .ConfigureAwait(false);
        if (!result.IsSuccess || result.Data is null)
            return Response<PlatformOpsStats>.Failure(
                result.ErrorMessage ?? "Could not load platform stats.",
                result.StatusCode);

        var dto = result.Data;
        return Response<PlatformOpsStats>.Success(new PlatformOpsStats
        {
            HospitalCount = dto.HospitalCount,
            PatientCount = dto.PatientCount,
            DoctorCount = dto.DoctorCount,
            StaffCount = dto.StaffCount,
            FacilityCount = dto.FacilityCount,
            DeviceCount = dto.DeviceCount,
            UnassignedDeviceCount = dto.UnassignedDeviceCount,
            AssignedDeviceCount = dto.AssignedDeviceCount,
            AppointmentsTodayCount = dto.AppointmentsTodayCount,
            ActiveAdmissionsCount = dto.ActiveAdmissionsCount,
            PendingStaffInvitationCount = dto.PendingStaffInvitationCount,
            EmergencyEventsLast72HoursCount = dto.EmergencyEventsLast72HoursCount
        });
    }

    private sealed class PlatformOpsStatsDto
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
}
