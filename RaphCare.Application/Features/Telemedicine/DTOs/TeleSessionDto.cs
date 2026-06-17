using RaphCare.Application.Common.DTOs;

namespace RaphCare.Application.Features.Telemedicine.DTOs;

public class TeleSessionDto : BaseDto
{
    public Guid ClinicId { get; set; }
    public Guid AppointmentId { get; set; }
    public Guid VisitId { get; set; }
    public Guid PatientId { get; set; }
    public Guid ProviderId { get; set; }
    public DateTime ScheduledStart { get; set; }
    public DateTime? ActualStart { get; set; }
    public DateTime? ActualEnd { get; set; }
    public string Status { get; set; } = string.Empty;
}

