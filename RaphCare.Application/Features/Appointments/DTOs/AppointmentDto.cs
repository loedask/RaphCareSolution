using RaphCare.Application.Common.DTOs;

namespace RaphCare.Application.Features.Appointments.DTOs;

public class AppointmentDto : BaseDto
{
    public Guid ClinicId { get; set; }
    public Guid PatientId { get; set; }
    public Guid ProviderId { get; set; }
    public DateTime ScheduledStart { get; set; }
    public DateTime ScheduledEnd { get; set; }
    public string Status { get; set; } = string.Empty;
}

