namespace RaphCare.Application.Features.Organization.DTOs;

public sealed class AdminClinicDashboardDto
{
    public Guid ClinicId { get; set; }
    public string ClinicName { get; set; } = string.Empty;
    public int PatientCount { get; set; }
    public int StaffCount { get; set; }
    public int ProviderCount { get; set; }
    public int FacilityCount { get; set; }
    public int AppointmentsTodayCount { get; set; }
    public int UpcomingAppointmentsCount { get; set; }
    public IReadOnlyList<AdminClinicAppointmentListItemDto> UpcomingAppointments { get; set; } = Array.Empty<AdminClinicAppointmentListItemDto>();
}
